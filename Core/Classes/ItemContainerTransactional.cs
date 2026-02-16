using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kryz.Utils;

namespace Kryz.RPG.Items
{
	public abstract class ItemContainerTransactional : IItemContainerTransactional
	{
		private const string indexOutOfRangeMsg = "Index was out of range. Must be non-negative and less than the size of the collection.";

		private readonly ArrayPool<ItemStack> arrayPool;

		private ItemStack[] slots;
		private int size;
		private int occupiedSlots;

		public event IItemContainerTransactional.OnSlotChangedDelegate? OnSlotChanged;
		public event IItemContainerTransactional.OnSizeChangedDelegate? OnSizeChanged;
		public event Action? OnTransactionComplete;

		public ItemStack this[int index] => (uint)index >= (uint)size ? throw new IndexOutOfRangeException(indexOutOfRangeMsg) : slots[index];
		public ReadOnlySpan<ItemStack> Slots => new(slots, 0, size);
		public int Size => size;
		public int FreeSlots => size - occupiedSlots;
		public int OccupiedSlots => occupiedSlots;

		int IReadOnlyCollection<ItemStack>.Count => size;

		public ItemContainerTransactional(int size, ArrayPool<ItemStack>? pool = null)
		{
			arrayPool = pool ?? ArrayPool<ItemStack>.Shared;
			slots = null!;
			SetSize(size);
		}

		public virtual int GetMaxStacks(IItem item) => item.MaxStacks;

		public virtual bool CanReceive(int index, IItem item) => true;

		public virtual bool CanRemove(int index) => true;

		public bool CanAdd(in ItemStack stack, out ItemStack remainder)
		{
			remainder = stack;

			if (stack.Item == null)
			{
				return false;
			}

			int maxCount = GetMaxStacks(stack.Item);

			for (int i = 0; i < size && remainder.Count > 0; i++)
			{
				if (CanReceive(i, stack.Item))
				{
					slots[i].TryAdd(remainder, out _, out remainder, maxCount);
				}
			}

			return remainder.Count < stack.Count; // Return true if we added at least 1 item
		}

		public bool CanAddSlot(int index, in ItemStack stack, out ItemStack result, out ItemStack remainder)
		{
			ItemStack slot = slots[index];
			remainder = stack;
			result = slot;

			return stack.Item != null
				&& CanReceive(index, stack.Item)
				&& slot.TryAdd(stack, out result, out remainder, GetMaxStacks(stack.Item));
		}

		public bool CanRemove(in ItemStack stack)
		{
			if (stack.Item == null)
			{
				return false;
			}

			int required = stack.Count;

			for (int i = 0; i < size && required > 0; i++)
			{
				ItemStack slot = slots[i];

				if (slot.Item == stack.Item && CanRemove(i))
				{
					required -= slot.Count;
				}
			}

			return required <= 0;
		}

		public bool CanRemoveSlot(int index, out ItemStack result, out ItemStack removed, int count = -1)
		{
			ItemStack slot = slots[index];
			removed = default;
			result = slot;
			return CanRemove(index) && slot.TryRemove(count < 0 ? slot.Count : count, out result, out removed);
		}

		public bool CanMove(int fromIndex, IReadOnlyItemContainer to, int count = -1)
		{
			return CanMove(this, fromIndex, to, count);
		}

		public bool CanMoveSlot(int fromIndex, IReadOnlyItemContainer to, int toIndex, int count = -1)
		{
			return CanMoveSlot(this, fromIndex, to, toIndex, count);
		}

		public bool CanSetSlot(int index, in ItemStack stack)
		{
			return stack.Item == null || CanReceive(index, stack.Item) && stack.Count < GetMaxStacks(stack.Item);
		}

		public bool CanSetSize(int value)
		{
			if (value < 0) return false;

			for (int i = value; i < size; i++) // If we're shrinking the container
			{
				if (!slots[i].IsEmpty) // But there are items in the slots that would be removed
				{
					return false; // Don't shrink
				}
			}

			return true;
		}

		public bool TryAdd<T>(T transaction, in ItemStack stack, out ItemStack remainder) where T : IItemTransaction
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

				if (slot.Item == stack.Item && CanReceive(i, stack.Item) && slot.TryAdd(remainder, out ItemStack result, out remainder, maxCount))
				{
					SetSlotRef(transaction, ref slot, i, result);
				}
			}

			// Then, try adding to empty slots
			for (int i = 0; i < size && remainder.Count > 0; i++)
			{
				ref ItemStack slot = ref slots[i];

				if (slot.Item == null && CanReceive(i, stack.Item) && slot.TryAdd(remainder, out ItemStack result, out remainder, maxCount))
				{
					SetSlotRef(transaction, ref slot, i, result);
				}
			}

			return remainder.Count < stack.Count; // Return true if we added at least 1 item
		}

		public bool TryAddSlot<T>(T transaction, int index, in ItemStack stack, out ItemStack result, out ItemStack remainder) where T : IItemTransaction
		{
			ref ItemStack slot = ref slots[index];

			if (stack.Item != null && CanReceive(index, stack.Item) && slot.TryAdd(stack, out result, out remainder, GetMaxStacks(stack.Item)))
			{
				SetSlotRef(transaction, ref slot, index, result);
				return true;
			}

			remainder = stack;
			result = slot;
			return false;
		}

		public bool TryRemove<T>(T transaction, in ItemStack stack) where T : IItemTransaction
		{
			if (stack.Item == null)
			{
				return false;
			}

			int required = stack.Count;

			for (int i = 0; i < size && required > 0; i++)
			{
				ref ItemStack slot = ref slots[i];

				if (slot.Item == stack.Item && CanRemove(i) && slot.TryRemove(Math.Min(required, slot.Count), out ItemStack result, out ItemStack removed))
				{
					required -= removed.Count;
					SetSlotRef(transaction, ref slot, i, result);
				}
			}

			return required <= 0;
		}

		public bool TryRemoveSlot<T>(T transaction, int index, out ItemStack result, out ItemStack removed, int count = -1) where T : IItemTransaction
		{
			ref ItemStack slot = ref slots[index];

			if (CanRemove(index) && slot.TryRemove(count < 0 ? slot.Count : count, out result, out removed))
			{
				SetSlotRef(transaction, ref slot, index, result);
				return true;
			}

			removed = default;
			result = slot;
			return false;
		}

		public bool TryMove<T>(T transaction, int fromIndex, IItemContainerTransactional to, int count = -1) where T : IItemTransaction
		{
			return TryMove(transaction, this, fromIndex, to, count);
		}

		public bool TryMoveSlot<T>(T transaction, int fromIndex, IItemContainerTransactional to, int toIndex, int count = -1) where T : IItemTransaction
		{
			return TryMoveSlot(transaction, this, fromIndex, to, toIndex, count);
		}

		public bool TrySetSize<T>(T transaction, int value) where T : IItemTransaction
		{
			if (CanSetSize(value))
			{
				SetSize(transaction, value);
				return true;
			}
			return false;
		}

		public bool TrySetSlot<T>(T transaction, int index, in ItemStack stack) where T : IItemTransaction
		{
			if (stack.Item == null || CanReceive(index, stack.Item) && stack.Count < GetMaxStacks(stack.Item))
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

		public void Sort<T>(T transaction) where T : IItemTransaction => Sort(transaction, ValueComparer<ItemStack>.Default);

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

			OnTransactionComplete?.Invoke();
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
		private void SetSlotRef<T>(T transaction, ref ItemStack slot, int index, in ItemStack value) where T : IItemTransaction
		{
			transaction.RecordSlot(this, index, slot, value);
			SetSlotRef(ref slot, value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void SetSlotRef(ref ItemStack slot, in ItemStack value)
		{
			if (slot.Count <= 0 && value.Count > 0) occupiedSlots++;
			else if (slot.Count > 0 && value.Count <= 0) occupiedSlots--;
			slot = value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void SetSize<T>(T transaction, int value) where T : IItemTransaction
		{
			transaction.RecordSize(this, size, value);
			SetSize(value);
		}

		private void SetSize(int value)
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

		public static bool CanAddSlot(IReadOnlyItemContainer container, int index, ItemStack slot, ItemStack stack, out ItemStack remainder)
		{
			remainder = stack;
			return stack.Item != null && container.CanReceive(index, stack.Item) && slot.TryAdd(stack, out remainder, out stack, container.GetMaxStacks(stack.Item));
		}

		public static bool CanMove(IReadOnlyItemContainer from, int fromIndex, IReadOnlyItemContainer to, int count = -1)
		{
			if (!from.CanRemoveSlot(fromIndex, out ItemStack result, out ItemStack removed, count))
			{
				return false;
			}

			if (to.CanAdd(removed, out ItemStack rem))
			{
				return rem.IsEmpty || CanAddSlot(from, fromIndex, result, rem, out rem) && rem.IsEmpty;
			}

			return false;
		}

		public static bool CanMoveSlot(IReadOnlyItemContainer from, int fromIndex, IReadOnlyItemContainer to, int toIndex, int count = -1)
		{
			if (!from.CanRemoveSlot(fromIndex, out ItemStack result, out ItemStack fromSlot, count))
			{
				return false;
			}

			if (to.CanAddSlot(toIndex, fromSlot, out _, out ItemStack rem))
			{
				return rem.IsEmpty || CanAddSlot(from, fromIndex, result, rem, out rem) && rem.IsEmpty;
			}
			else if (result.IsEmpty && to.CanRemoveSlot(toIndex, out ItemStack toResult, out ItemStack toSlot))
			{
				return CanAddSlot(from, fromIndex, result, toSlot, out rem) && rem.IsEmpty
					&& CanAddSlot(to, toIndex, toResult, fromSlot, out rem) && rem.IsEmpty;
			}

			return false;
		}

		public static bool TryMove<T>(T transaction, IItemContainerTransactional from, int fromIndex, IItemContainerTransactional to, int count = -1) where T : IItemTransaction
		{
			if (!from.TryRemoveSlot(transaction, fromIndex, out _, out ItemStack removed, count))
			{
				return false;
			}

			if (to.TryAdd(transaction, removed, out ItemStack rem))
			{
				return rem.IsEmpty || from.TryAddSlot(transaction, fromIndex, rem, out _, out rem) && rem.IsEmpty;
			}

			return false;
		}

		public static bool TryMoveSlot<T>(T transaction, IItemContainerTransactional from, int fromIndex, IItemContainerTransactional to, int toIndex, int count = -1) where T : IItemTransaction
		{
			if (!from.TryRemoveSlot(transaction, fromIndex, out ItemStack result, out ItemStack fromSlot, count))
			{
				return false;
			}

			if (to.TryAddSlot(transaction, toIndex, fromSlot, out _, out ItemStack rem))
			{
				return rem.IsEmpty || from.TryAddSlot(transaction, fromIndex, rem, out _, out rem) && rem.IsEmpty;
			}
			else if (result.IsEmpty && to.TryRemoveSlot(transaction, toIndex, out _, out ItemStack toSlot))
			{
				return from.TryAddSlot(transaction, fromIndex, toSlot, out _, out rem) && rem.IsEmpty
					&& to.TryAddSlot(transaction, toIndex, fromSlot, out _, out rem) && rem.IsEmpty;
			}

			return false;
		}
	}
}