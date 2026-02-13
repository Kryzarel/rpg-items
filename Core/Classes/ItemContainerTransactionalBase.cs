using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kryz.Utils;

namespace Kryz.RPG.Items
{
	public abstract class ItemContainerTransactionalBase : IItemContainerTransactional
	{
		private const string indexOutOfRangeMsg = "Index was out of range. Must be non-negative and less than the size of the collection.";

		protected readonly ArrayPool<ItemStack> arrayPool;

		protected ItemStack[] slots;
		protected int size;
		protected int occupiedSlots;

		public event IItemContainerTransactional.OnSlotChangedDelegate? OnSlotChanged;
		public event IItemContainerTransactional.OnSizeChangedDelegate? OnSizeChanged;
		public event Action? OnChangesFinished;

		public ItemStack this[int index] => (uint)index >= (uint)size ? throw new IndexOutOfRangeException(indexOutOfRangeMsg) : slots[index];
		public ReadOnlySpan<ItemStack> Slots => new(slots, 0, size);
		public int Size => size;
		public int FreeSlots => size - occupiedSlots;
		public int OccupiedSlots => occupiedSlots;

		int IReadOnlyCollection<ItemStack>.Count => size;

		public ItemContainerTransactionalBase() : this(0) { }

		public ItemContainerTransactionalBase(int size, ArrayPool<ItemStack>? pool = null)
		{
			arrayPool = pool ?? ArrayPool<ItemStack>.Shared;
			slots = null!;
			SetSize(size);
		}

		public virtual int GetMaxStacks(IItem item) => item.MaxStacks;

		public virtual bool CanReceive(int index, in ItemStack stack) => true;

		public virtual bool CanRemove(int index) => true;

		public virtual bool TryAdd<T>(T transaction, in ItemStack stack, out ItemStack remainder) where T : IItemTransaction
		{
			remainder = stack;

			if (stack.Item == null)
			{
				return false;
			}

			int maxCount = GetMaxStacks(stack.Item);

			// First, try adding to slots that already have the same item
			for (int i = 0; i < size && remainder.Count > 0; i++)
			{
				ref ItemStack slot = ref slots[i];

				if (slot.Item == stack.Item && CanReceive(i, remainder) && slot.TryAdd(remainder, out ItemStack result, out remainder, maxCount))
				{
					SetSlotRef(transaction, ref slot, i, result);
				}
			}

			// Then, try adding to empty slots
			for (int i = 0; i < size && remainder.Count > 0; i++)
			{
				ref ItemStack slot = ref slots[i];

				if (slot.Item == null && CanReceive(i, remainder) && slot.TryAdd(remainder, out ItemStack result, out remainder, maxCount))
				{
					SetSlotRef(transaction, ref slot, i, result);
				}
			}

			return remainder.Count < stack.Count; // Return true if we added at least 1 item
		}

		public virtual bool TryAddSlot<T>(T transaction, int index, in ItemStack stack, out ItemStack result, out ItemStack remainder) where T : IItemTransaction
		{
			ref ItemStack slot = ref slots[index];

			if (stack.Item != null && CanReceive(index, stack) && slot.TryAdd(stack, out result, out remainder, GetMaxStacks(stack.Item)))
			{
				SetSlotRef(transaction, ref slot, index, result);
				return true;
			}

			remainder = stack;
			result = slot;
			return false;
		}

		public virtual bool TryRemove<T>(T transaction, in ItemStack stack) where T : IItemTransaction
		{
			if (stack.Item == null)
			{
				return false;
			}

			for (int i = 0, required = stack.Count; i < size; i++)
			{
				ref ItemStack slot = ref slots[i];

				if (slot.Item == stack.Item && CanRemove(i) && slot.TryRemove(Math.Min(required, slot.Count), out ItemStack result, out ItemStack removed))
				{
					SetSlotRef(transaction, ref slot, i, result);

					required -= removed.Count;
					if (required <= 0)
					{
						return true;
					}
				}
			}

			return false;
		}

		public virtual bool TryRemoveSlot<T>(T transaction, int index, out ItemStack result, out ItemStack removed, int count = -1) where T : IItemTransaction
		{
			ref ItemStack slot = ref slots[index];
			if (count < 0) count = slot.Count;

			if (CanRemove(index) && slot.TryRemove(count, out result, out removed))
			{
				SetSlotRef(transaction, ref slot, index, result);
				return true;
			}

			removed = default;
			result = slot;
			return false;
		}

		public virtual bool TrySetSize<T>(T transaction, int value) where T : IItemTransaction
		{
			if (value < 0) value = 0;

			if (value == size)
			{
				return false;
			}

			for (int i = value; i < size; i++) // If we're shrinking the container
			{
				if (!slots[i].IsEmpty) // But there are items in the slots that would be removed
				{
					return false; // Don't shrink
				}
			}

			SetSize(transaction, value);
			return true;
		}

		public virtual bool TrySetSlot<T>(T transaction, int index, in ItemStack stack) where T : IItemTransaction
		{
			if (CanReceive(index, stack))
			{
				SetSlot(transaction, index, stack);
				return true;
			}
			return false;
		}

		public void SetSlot<T>(T transaction, int index, in ItemStack stack) where T : IItemTransaction
		{
			ref ItemStack slot = ref slots[index];
			if (slot != stack)
			{
				SetSlotRef(transaction, ref slot, index, stack);
			}
		}

		public void Sort<T>(T transaction) where T : IItemTransaction
		{
			Sort(transaction, ValueComparer<ItemStack>.Default);
		}

		public void Sort<T, TComparer>(T transaction, TComparer comparer) where T : IItemTransaction where TComparer : IComparer<ItemStack>
		{
			ItemStack[] sortedArray = arrayPool.Rent(size);
			Array.Copy(slots, sortedArray, size);
			Span<ItemStack> sorted = new(sortedArray, 0, size);

			// We DON'T use the default Array.Sort because it is unstable. I think stable sorting is very valuable here.
			// Our version also tends to be just as fast, if not slightly faster, which is nice.
			TimSortLite.Sort(sorted, comparer);

			for (int i = 0; i < size; i++)
			{
				SetSlot(transaction, i, sorted[i]);
			}

			sorted.Clear();
			arrayPool.Return(sortedArray);
		}

		public void Notify(ReadOnlySpan<SlotEdit> slotEdits, SizeEdit? sizeEdit)
		{
			if (sizeEdit.HasValue)
			{
				SizeEdit edit = sizeEdit.Value;
				OnSizeChanged?.Invoke(edit.OldSize, edit.NewSize);
			}

			for (int i = 0; i < slotEdits.Length; i++)
			{
				ref readonly SlotEdit edit = ref slotEdits[i];

				if ((uint)edit.Index < (uint)slots.Length)
				{
					OnSlotChanged?.Invoke(edit.Index, edit.OldStack, edit.NewStack);
				}
			}

			OnChangesFinished?.Invoke();
		}

		public void Rollback(ReadOnlySpan<SlotEdit> slotEdits, SizeEdit? sizeEdit)
		{
			if (sizeEdit.HasValue)
			{
				SetSize(sizeEdit.Value.OldSize);
			}

			for (int i = 0; i < slotEdits.Length; i++)
			{
				ref readonly SlotEdit edit = ref slotEdits[i];

				if ((uint)edit.Index < (uint)slots.Length)
				{
					SetSlotRef(ref slots[edit.Index], edit.OldStack);
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void SetSlotRef<T>(T transaction, ref ItemStack slot, int index, in ItemStack value) where T : IItemTransaction
		{
			transaction.RecordSlot(this, index, slot, value);
			SetSlotRef(ref slot, value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void SetSlotRef(ref ItemStack slot, in ItemStack value)
		{
			if (slot.Count <= 0 && value.Count > 0) occupiedSlots++;
			else if (slot.Count > 0 && value.Count <= 0) occupiedSlots--;
			slot = value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void SetSize<T>(T transaction, int size) where T : IItemTransaction
		{
			transaction.RecordSize(this, size);
			SetSize(size);
		}

		protected void SetSize(int value)
		{
			if (value < 0) value = 0;
			if (value == size && slots != null) return;

			if (slots == null || value > slots.Length)
			{
				ItemStack[]? oldSlots = slots;
				slots = value > 0 ? arrayPool.Rent(value) : Array.Empty<ItemStack>();

				// We SHOULD NOT return an empty array to the pool
				if (oldSlots != null && oldSlots.Length > 0)
				{
					Array.Copy(oldSlots, slots, size);
					Array.Clear(oldSlots, 0, size);
					arrayPool.Return(oldSlots);
				}
			}
			else if (value < size)
			{
				Array.Clear(slots, value, size);
			}

			size = value;
		}

		public ArraySegment<ItemStack>.Enumerator GetEnumerator() => new ArraySegment<ItemStack>(slots, 0, size).GetEnumerator();

		IEnumerator<ItemStack> IEnumerable<ItemStack>.GetEnumerator() => GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}