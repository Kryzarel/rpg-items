using System;
using System.Buffers;
using System.Collections.Generic;
using Kryz.Utils;

namespace Kryz.RPG.Items
{
	public class ItemContainer : IItemContainerTransactional, IItemContainerMutable, IItemContainer
	{
		protected readonly ArrayPool<ItemStack> arrayPool;

		protected ItemStack[] slots;
		protected int size;
		protected int occupiedSlots;

		public event IItemContainer.OnSlotChangedDelegate OnSlotChanged = delegate { };
		public event IItemContainer.OnSizeChangedDelegate OnSizeChanged = delegate { };

		public ItemStack this[int index] => ((IItemContainerMutable)this)[index];
		public ReadOnlySpan<ItemStack> Slots => new(slots, 0, size);
		public int Size => size;
		public int FreeSlots => size - occupiedSlots;
		public int OccupiedSlots => occupiedSlots;

		Span<ItemStack> IItemContainerMutable.Slots => new(slots, 0, size);
		int IItemContainerMutable.Size { get => Size; set => SetSize(value); }

		ref ItemStack IItemContainerMutable.this[int index]
		{
			get
			{
				if ((uint)index >= (uint)size)
					throw new IndexOutOfRangeException();
				return ref slots[index];
			}
		}

		public ItemContainer(int size, ArrayPool<ItemStack>? pool = null)
		{
			arrayPool = pool ?? ArrayPool<ItemStack>.Shared;
			slots = null!;
			SetSize(size);
		}

		public int GetMaxStacks(IItem item) => item.MaxStacks;

		public virtual bool CanReceive(int index, in ItemStack stack) => true;

		public bool CanRemove(int index) => true;

		public bool TryAdd(ItemTransaction transaction, in ItemStack stack, out ItemStack remainder)
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

				if (slot.Item == stack.Item && CanReceive(i, stack) && slot.TryAdd(stack, out ItemStack result, out remainder, maxCount))
				{
					transaction.RecordSlot(this, i, result);
					slot = result;
				}
			}

			// Then, try adding to empty slots
			for (int i = 0; i < size && remainder.Count > 0; i++)
			{
				ref ItemStack slot = ref slots[i];

				if (slot.Item == null && CanReceive(i, stack) && slot.TryAdd(stack, out ItemStack result, out remainder, maxCount))
				{
					transaction.RecordSlot(this, i, result);
					slot = result;
				}
			}

			return remainder.Count < stack.Count; // Return true if we added at least 1 item
		}

		public bool TryAdd(ItemTransaction transaction, int index, in ItemStack stack, out ItemStack result, out ItemStack remainder)
		{
			ref ItemStack slot = ref slots[index];

			if (stack.Item != null && CanReceive(index, stack) && slot.TryAdd(stack, out result, out remainder, GetMaxStacks(stack.Item)))
			{
				transaction.RecordSlot(this, index, result);
				slot = result;
				return true;
			}

			remainder = stack;
			result = slot;
			return false;
		}

		public bool TryRemove(ItemTransaction transaction, in ItemStack stack)
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
					transaction.RecordSlot(this, i, result);
					slot = result;

					required -= removed.Count;
					if (required <= 0)
					{
						return true;
					}
				}
			}

			return false;
		}

		public bool TryRemove(ItemTransaction transaction, int index, int count, out ItemStack result, out ItemStack removed)
		{
			ref ItemStack slot = ref slots[index];
			if (count < 0) count = slot.Count;

			if (CanRemove(index) && slot.TryRemove(count, out result, out removed))
			{
				transaction.RecordSlot(this, index, result);
				slot = result;
				return true;
			}

			removed = default;
			result = slot;
			return false;
		}

		public bool TrySetSize(ItemTransaction transaction, int value)
		{
			if (value == size)
			{
				return false;
			}

			// If we're shrinking the container
			if (value < size)
			{
				for (int i = value; i < size; i++)
				{
					// But there are items in the slots that would be removed
					if (!slots[i].IsEmpty)
					{
						// Don't shrink
						return false;
					}
				}
			}

			transaction.RecordSize(this, value);
			SetSize(value);
			return true;
		}

		public bool TrySetSlot(ItemTransaction transaction, int index, in ItemStack stack)
		{
			if (CanReceive(index, stack))
			{
				ref ItemStack slot = ref slots[index];
				if (slot != stack)
				{
					transaction.RecordSlot(this, index, stack);
					slot = stack;
				}
				return true;
			}
			return false;
		}

		protected virtual void SetSize(int value)
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

		public void NotifySlotChanged(int index, ItemStack oldStack, ItemStack newStack)
		{
			if (oldStack.Count <= 0 && newStack.Count > 0) occupiedSlots++;
			else if (oldStack.Count > 0 && newStack.Count <= 0) occupiedSlots--;
			OnSlotChanged(index, oldStack, newStack);
		}

		public void NotifySizeChanged(int oldSize, int newSize)
		{
			OnSizeChanged(oldSize, newSize);
		}

		public bool TrySetSize(int value)
		{
			using ItemTransaction transaction = new(this);
			if (!TrySetSize(transaction, value))
			{
				return false;
			}
			transaction.Commit();
			return true;
		}

		public bool TryAdd(in ItemStack stack, out ItemStack remainder)
		{
			using ItemTransaction transaction = new(this);
			if (!TryAdd(transaction, stack, out remainder))
			{
				return false;
			}
			transaction.Commit();
			return true;
		}

		public bool TryAdd(int index, in ItemStack stack, out ItemStack result, out ItemStack remainder)
		{
			using ItemTransaction transaction = new(this);
			if (!TryAdd(transaction, index, stack, out result, out remainder))
			{
				return false;
			}
			transaction.Commit();
			return true;
		}

		public bool TryRemove(in ItemStack stack)
		{
			using ItemTransaction transaction = new(this);
			if (!TryRemove(transaction, stack))
			{
				return false;
			}
			transaction.Commit();
			return true;
		}

		public bool TryRemove(int index, int count, out ItemStack result, out ItemStack removed)
		{
			using ItemTransaction transaction = new(this);
			if (!TryRemove(transaction, index, count, out result, out removed))
			{
				return false;
			}
			transaction.Commit();
			return true;
		}

		public bool TryMove(int fromIndex, int count, IItemContainer to)
		{
			using ItemTransaction transaction = new(this);

			if (!TryRemove(transaction, fromIndex, count, out _, out ItemStack removed))
			{
				return false;
			}

			if (!to.TryAdd(transaction, removed, out ItemStack remainder))
			{
				return false;
			}

			if (!remainder.IsEmpty && (!TryAdd(transaction, fromIndex, remainder, out _, out ItemStack rem) || !rem.IsEmpty))
			{
				return false;
			}

			transaction.Commit();
			return true;
		}

		public bool TryMove(int fromIndex, int count, IItemContainer to, int toIndex)
		{
			using ItemTransaction transaction = new(this);

			if (!TryRemove(transaction, fromIndex, count, out ItemStack result, out ItemStack fromSlot))
			{
				return false;
			}

			if (to.TryAdd(transaction, toIndex, fromSlot, out _, out ItemStack remainder))
			{
				if (remainder.IsEmpty || TryAdd(transaction, fromIndex, remainder, out _, out remainder) && remainder.IsEmpty)
				{
					transaction.Commit();
					return true;
				}
			}
			else if (result.IsEmpty && to.TryRemove(transaction, toIndex, -1, out _, out ItemStack toSlot))
			{
				if (!TryAdd(transaction, fromIndex, toSlot, out _, out remainder) || !remainder.IsEmpty)
				{
					return false;
				}

				if (!to.TryAdd(transaction, toIndex, fromSlot, out _, out remainder) || !remainder.IsEmpty)
				{
					return false;
				}

				transaction.Commit();
				return true;
			}

			return false;
		}

		public void CopyFrom(ReadOnlySpan<ItemStack> stacks)
		{
			using ItemTransaction transaction = new(this);
			int count = stacks.Length;

			if (size < count)
			{
				transaction.RecordSize(this, count);
				SetSize(count);
			}

			for (int i = 0; i < count; i++)
			{
				ItemStack stack = stacks[i];
				ref ItemStack slot = ref slots[i];

				if (slot != stack)
				{
					transaction.RecordSlot(this, i, stack);
					slot = stack;
				}
			}

			transaction.Commit();
		}

		public void CopyFrom(IReadOnlyList<ItemStack> stacks)
		{
			using ItemTransaction transaction = new(this);
			int count = stacks.Count;

			if (size < count)
			{
				transaction.RecordSize(this, count);
				SetSize(count);
			}

			for (int i = 0; i < count; i++)
			{
				ItemStack stack = stacks[i];
				ref ItemStack slot = ref slots[i];

				if (slot != stack)
				{
					transaction.RecordSlot(this, i, stack);
					slot = stack;
				}
			}

			transaction.Commit();
		}

		public void Clear()
		{
			using ItemTransaction transaction = new(this);

			for (int i = 0; i < size; i++)
			{
				ref ItemStack slot = ref slots[i];

				if (slot != default)
				{
					transaction.RecordSlot(this, i, default);
					slot = default;
				}
			}

			transaction.Commit();
		}

		public void Sort() => Sort(ValueComparer<ItemStack>.Default);

		public void Sort<TComparer>(TComparer comparer) where TComparer : IComparer<ItemStack>
		{
			using ItemTransaction transaction = new(this);
			ItemStack[] original = arrayPool.Rent(size);
			Array.Copy(slots, original, size);

			// We DON'T use the default Array.Sort because it is unstable. I think stable sorting is very valuable here.
			// Our version also tends to be just as fast, if not slightly faster, which is nice.
			TimSortLite.Sort(slots.AsSpan(0, size), comparer);

			for (int i = 0; i < size; i++)
			{
				ItemStack slot = slots[i];
				ItemStack originalSlot = original[i];

				if (slot != originalSlot)
				{
					transaction.RecordSlot(this, i, slot);
				}
			}

			Array.Clear(original, 0, size);
			arrayPool.Return(original);
			transaction.Commit();
		}
	}
}