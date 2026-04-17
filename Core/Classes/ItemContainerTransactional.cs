using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Kryz.Utils;

namespace Kryz.RPG.Items
{
	public abstract class ItemContainerTransactional : IItemContainerTransactional, IDisposable
	{
		private const string outOfRange = "Index was out of range. Must be non-negative and less than the size of the collection.";

		private readonly ArrayPool<ItemStack> arrayPool;

		private ItemStack[] slots;
		private int size;
		private int occupiedSlots;

		public event IItemContainerTransactional.OnSlotChangedDelegate? OnSlotChanged;
		public event IItemContainerTransactional.OnSizeChangedDelegate? OnSizeChanged;
		public event IItemContainerTransactional.OnTransactionSuccessDelegate? OnTransactionSuccess;

		private static int nextId;
		public int InstanceId { get; } = Interlocked.Increment(ref nextId);

		public ItemStack this[int index] => (uint)index >= (uint)size ? throw new IndexOutOfRangeException(outOfRange) : slots[index];
		public ReadOnlySpan<ItemStack> Slots => new(slots, 0, size);
		public int Size => size;
		public int FreeSlots => size - occupiedSlots;
		public int OccupiedSlots => occupiedSlots;

		int IReadOnlyCollection<ItemStack>.Count => size;

		public ItemContainerTransactional(int size, ArrayPool<ItemStack>? pool = null)
		{
			arrayPool = pool ?? ArrayPool<ItemStack>.Shared;
			slots = size > 0 ? arrayPool.Rent(size) : Array.Empty<ItemStack>();
			this.size = size > 0 ? size : 0;
		}

		public virtual int GetMaxStacks(IItem item) => item.MaxStacks;

		public virtual bool CanReceive(int index, IItem item) => true;

		public virtual bool CanRemove(int index) => true;

		public virtual bool CanAdd(in ItemStack stack, out ItemStack remainder)
		{
			remainder = stack;

			if (stack.Item == null)
			{
				return false;
			}

			int maxCount = GetMaxStacks(stack.Item);

			for (int i = 0; i < size && remainder.Count > 0; i++)
			{
				ref ItemStack slot = ref slots[i];

				if (CanReceive(i, stack.Item))
				{
					// Call try add but don't assign the result to anything
					slot.TryAdd(remainder, out _, out remainder, maxCount);
				}
			}

			return remainder.Count < stack.Count; // Return true if we added at least 1 item
		}

		public virtual bool CanAddSlot(int index, in ItemStack stack, out ItemStack result, out ItemStack remainder)
		{
			ItemStack slot = slots[index];

			if (stack.Item != null && CanReceive(index, stack.Item))
			{
				return slot.TryAdd(stack, out result, out remainder, GetMaxStacks(stack.Item));
			}

			remainder = stack;
			result = slot;
			return false;
		}

		public virtual bool CanRemove(in ItemStack stack)
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

		public virtual bool CanRemoveSlot(int index, out ItemStack result, out ItemStack removed, int count = -1)
		{
			ItemStack slot = slots[index];

			if (CanRemove(index))
			{
				return slot.TryRemove(count < 0 ? slot.Count : count, out result, out removed);
			}

			removed = default;
			result = slot;
			return false;
		}

		public virtual bool CanMove(int fromIndex, IItemContainer to, int count = -1)
		{
			return CanRemoveSlot(fromIndex, out _, out ItemStack removed, count) && to.CanAdd(removed, out _);
		}

		public virtual bool CanMoveSlot(int fromIndex, IItemContainer to, int toIndex, int count = -1)
		{
			if (!CanRemoveSlot(fromIndex, out ItemStack result, out ItemStack removed, count))
			{
				return false;
			}

			if (to.CanAddSlot(toIndex, removed, out _, out _))
			{
				return true;
			}
			else if (result.IsEmpty && to.CanRemoveSlot(toIndex, out _, out ItemStack toSlot))
			{
				return CanSetSlot(fromIndex, toSlot) && to.CanSetSlot(toIndex, removed);
			}

			return false;
		}

		public virtual bool CanSetSlot(int index, in ItemStack stack)
		{
			return stack.Item == null || CanReceive(index, stack.Item) && stack.Count <= GetMaxStacks(stack.Item);
		}

		public virtual bool CanSetSize(int value)
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

		public virtual bool TryAddSlot<T>(T transaction, int index, in ItemStack stack, out ItemStack result, out ItemStack remainder) where T : IItemTransaction
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

		public virtual bool TryRemove<T>(T transaction, in ItemStack stack) where T : IItemTransaction
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

		public virtual bool TryRemoveSlot<T>(T transaction, int index, out ItemStack result, out ItemStack removed, int count = -1) where T : IItemTransaction
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

		public virtual bool TryMove<T>(T transaction, int fromIndex, IItemContainerTransactional to, int count = -1) where T : IItemTransaction
		{
			if (!TryRemoveSlot(transaction, fromIndex, out _, out ItemStack removed, count))
			{
				return false;
			}

			if (to.TryAdd(transaction, removed, out ItemStack rem))
			{
				return rem.IsEmpty || TryAddSlot(transaction, fromIndex, rem, out _, out rem) && rem.IsEmpty;
			}

			return false;
		}

		public virtual bool TryMoveSlot<T>(T transaction, int fromIndex, IItemContainerTransactional to, int toIndex, int count = -1) where T : IItemTransaction
		{
			if (!TryRemoveSlot(transaction, fromIndex, out ItemStack result, out ItemStack fromSlot, count))
			{
				return false;
			}

			if (to.TryAddSlot(transaction, toIndex, fromSlot, out _, out ItemStack rem))
			{
				return rem.IsEmpty || TryAddSlot(transaction, fromIndex, rem, out _, out rem) && rem.IsEmpty;
			}
			else if (result.IsEmpty && to.TryRemoveSlot(transaction, toIndex, out _, out ItemStack toSlot))
			{
				return TryAddSlot(transaction, fromIndex, toSlot, out _, out rem) && rem.IsEmpty
					&& to.TryAddSlot(transaction, toIndex, fromSlot, out _, out rem) && rem.IsEmpty;
			}

			return false;
		}

		public virtual bool TrySetSlot<T>(T transaction, int index, in ItemStack stack) where T : IItemTransaction
		{
			if (CanSetSlot(index, stack))
			{
				SetSlot(transaction, index, stack);
				return true;
			}
			return false;
		}

		public virtual bool TrySetSize<T>(T transaction, int value) where T : IItemTransaction
		{
			if (CanSetSize(value))
			{
				SetSize(transaction, value);
				return true;
			}
			return false;
		}

		public virtual void SetSlot<T>(T transaction, int index, in ItemStack stack) where T : IItemTransaction
		{
			ref ItemStack slot = ref slots[index];
			if (slot != stack)
			{
				SetSlotRef(transaction, ref slot, index, stack);
			}
		}

		public virtual void Sort<T>(T transaction) where T : IItemTransaction => Sort(transaction, ValueComparer<ItemStack>.Default);

		public virtual void Sort<T, TComparer>(T transaction, TComparer comparer) where T : IItemTransaction where TComparer : IComparer<ItemStack>
		{
			ItemStack[] sortArray = arrayPool.Rent(size);
			Span<ItemStack> sortSpan = new(sortArray, 0, size);
			Slots.CopyTo(sortSpan);

			// We DON'T use the default Array.Sort because it is unstable. I think stable sorting is very valuable here.
			// Our version also tends to be just as fast, if not slightly faster, which is nice.
			TimSortLite.Sort(sortSpan, comparer);

			for (int i = 0; i < size; i++)
			{
				SetSlot(transaction, i, sortSpan[i]);
			}

			sortSpan.Clear();
			arrayPool.Return(sortArray);
		}

		public void NotifySize(int oldSize, int newSize)
		{
			OnSizeChanged?.Invoke(oldSize, newSize);
		}

		public void NotifySlot(int index, ItemStack oldStack, ItemStack newStack)
		{
			OnSlotChanged?.Invoke(index, oldStack, newStack);
		}

		public void NotifyTransactionSuccess()
		{
			OnTransactionSuccess?.Invoke();
		}

		public void RollbackSize(int oldSize)
		{
			SetSize(oldSize);
		}

		public void RollbackSlot(int index, ItemStack oldStack)
		{
			ref ItemStack slot = ref slots[index];
			UpdateDerivedValues(index, slot, oldStack);
			slot = oldStack;
		}

		protected virtual void UpdateDerivedValues(int index, ItemStack oldStack, ItemStack newStack)
		{
			if (oldStack.Count <= 0 && newStack.Count > 0) occupiedSlots++;
			else if (oldStack.Count > 0 && newStack.Count <= 0) occupiedSlots--;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void SetSlotRef<T>(T transaction, ref ItemStack slot, int index, in ItemStack value) where T : IItemTransaction
		{
			transaction.RecordSlot(this, index, slot, value);
			UpdateDerivedValues(index, slot, value);
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
			if (value == size) return;

			if (value > slots.Length)
			{
				ItemStack[]? oldSlots = slots;
				slots = arrayPool.Rent(value);

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
				Array.Clear(slots, value, size - value);
			}

			size = value;
		}

		public void Dispose()
		{
			if (slots != null)
			{
				Array.Clear(slots, 0, size);
				arrayPool.Return(slots);
				slots = null!;
				size = 0;
				occupiedSlots = 0;
			}
		}

		public ArraySegment<ItemStack>.Enumerator GetEnumerator() => new ArraySegment<ItemStack>(slots, 0, size).GetEnumerator();

		IEnumerator<ItemStack> IEnumerable<ItemStack>.GetEnumerator() => GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}