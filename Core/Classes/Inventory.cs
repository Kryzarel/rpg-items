using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kryz.Utils;

namespace Kryz.RPG.Items
{
	public class Inventory : IInventory, IDisposable
	{
		private readonly ArrayPool<ItemStack> arrayPool;

		private ItemStack[] slots;
		private int size;
		private int occupiedSlots;

		public event IInventory.OnSlotChangedDelegate OnSlotChanged = delegate { };
		public event IInventory.OnSizeChangedDelegate OnSizeChanged = delegate { };

		public ItemStack this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				if (index < 0 || index >= size)
					throw new IndexOutOfRangeException();
				return slots[index];
			}
		}

		public int Count => size;
		public int Size { get => size; set => SetSize(value); }
		public int FreeSlots => size - occupiedSlots;
		public int OccupiedSlots => occupiedSlots;

		public Inventory(int numSlots, ArrayPool<ItemStack>? pool = null)
		{
			arrayPool = pool ?? ArrayPool<ItemStack>.Shared;
			slots = null!;
			SetSize(numSlots);
		}

		public virtual int GetMaxStacks(IItem item) => item.MaxStacks;

		public virtual bool CanSwap(int index, in ItemStack stack, out ItemStack removed)
		{
			if (index >= 0 && index < size && (stack.Item == null || stack.Count <= GetMaxStacks(stack.Item)))
			{
				removed = slots[index];
				return true;
			}
			removed = default;
			return false;
		}

		public virtual bool CanAdd(in ItemStack stack, out ItemStack remainder, bool allowPartial = false)
		{
			remainder = stack;

			if (stack.Item == null)
			{
				return false;
			}

			int maxCount = GetMaxStacks(stack.Item);

			for (int i = 0; i < size && remainder.Count > 0; i++)
			{
				// Call ItemStack.TryAdd() but don't assign the result to anything.
				// This way we check what WOULD happen without actually changing the inventory.
				slots[i].TryAdd(remainder, out _, out remainder, maxCount);
			}

			if (remainder.Count < stack.Count && (remainder.Count == 0 || allowPartial))
			{
				return true;
			}

			remainder = stack;
			return false;
		}

		public virtual bool CanAdd(int index, in ItemStack stack, out ItemStack result, out ItemStack remainder, bool allowPartial = false)
		{
			if (index < 0 || index >= size)
			{
				remainder = stack;
				result = default;
				return false;
			}

			ItemStack slot = slots[index];

			if (stack.Item != null)
			{
				int maxCount = GetMaxStacks(stack.Item);

				if (slot.TryAdd(stack, out result, out remainder, maxCount) && (remainder.Count == 0 || allowPartial))
				{
					return true;
				}
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

			for (int i = 0, required = stack.Count; i < size; i++)
			{
				ItemStack slot = slots[i];

				if (slot.Item == stack.Item && (required -= slot.Count) <= 0)
				{
					return true;
				}
			}

			return false;
		}

		public virtual bool CanRemove(int index, out ItemStack result, out ItemStack removed)
		{
			return CanRemove(index, -1, out result, out removed);
		}

		public virtual bool CanRemove(int index, int count, out ItemStack result, out ItemStack removed)
		{
			if (index < 0 || index >= size)
			{
				result = default;
				removed = default;
				return false;
			}

			ItemStack slot = slots[index];
			if (count < 0) count = slot.Count;
			return slot.TryRemove(count, out result, out removed);
		}

		public virtual bool TrySwap(int index, in ItemStack stack, out ItemStack removed, bool forceSwap = false)
		{
			if (index >= 0 && index < size && (forceSwap || CanSwap(index, stack, out _)))
			{
				ref ItemStack slot = ref slots[index];
				removed = slot;

				if (slot != stack)
				{
					SetSlot(ref slot, index, stack);
				}
				return true;
			}

			removed = default;
			return false;
		}

		public virtual bool TryAdd(in ItemStack stack, out ItemStack remainder, bool allowPartial = false)
		{
			remainder = stack;

			if (stack.Item == null || !allowPartial && !CanAdd(stack, out _, allowPartial))
			{
				return false;
			}

			int maxCount = GetMaxStacks(stack.Item);

			// First, try adding to slots that already have the same item
			for (int i = 0; i < size && remainder.Count > 0; i++)
			{
				ref ItemStack slot = ref slots[i];

				if (slot.Item == stack.Item && slot.TryAdd(remainder, out ItemStack result, out remainder, maxCount))
				{
					SetSlot(ref slot, i, result);
				}
			}

			// Then, try adding to empty slots
			for (int i = 0; i < size && remainder.Count > 0; i++)
			{
				ref ItemStack slot = ref slots[i];

				if (slot.Item == null && slot.TryAdd(remainder, out ItemStack result, out remainder, maxCount))
				{
					SetSlot(ref slot, i, result);
				}
			}

			return remainder.Count < stack.Count; // Return true if we added at least 1 item
		}

		public virtual bool TryAdd(int index, in ItemStack stack, out ItemStack result, out ItemStack remainder, bool allowPartial = false)
		{
			if (index < 0 || index >= size)
			{
				remainder = stack;
				result = default;
				return false;
			}

			ref ItemStack slot = ref slots[index];

			if (stack.Item != null)
			{
				int maxCount = GetMaxStacks(stack.Item);

				if (slot.TryAdd(stack, out result, out remainder, maxCount) && (remainder.Count == 0 || allowPartial))
				{
					SetSlot(ref slot, index, result);
					return true;
				}
			}

			remainder = stack;
			result = slot;
			return false;
		}

		public virtual bool TryRemove(in ItemStack stack)
		{
			if (!CanRemove(stack))
			{
				return false;
			}

			for (int i = 0, required = stack.Count; i < size && required > 0; i++)
			{
				ref ItemStack slot = ref slots[i];

				if (slot.Item == stack.Item && slot.TryRemove(Math.Min(required, slot.Count), out ItemStack result, out ItemStack removed))
				{
					SetSlot(ref slot, i, result);
					required -= removed.Count;
				}
			}

			return true;
		}

		public virtual bool TryRemove(int index, out ItemStack result, out ItemStack removed)
		{
			return TryRemove(index, -1, out result, out removed);
		}

		public virtual bool TryRemove(int index, int count, out ItemStack result, out ItemStack removed)
		{
			if (index < 0 || index >= size)
			{
				removed = default;
				result = default;
				return false;
			}

			ref ItemStack slot = ref slots[index];
			if (count < 0) count = slot.Count;

			if (slot.TryRemove(count, out result, out removed))
			{
				SetSlot(ref slot, index, result);
				return true;
			}

			removed = default;
			result = slot;
			return false;
		}

		public virtual bool CanMove(int toIndex, IInventory fromInv, int fromIndex, bool allowSwap = false)
		{
			return IInventory.CanMove(this, toIndex, fromInv, fromIndex, allowSwap);
		}

		public virtual bool TryMove(int toIndex, IInventory fromInv, int fromIndex, bool allowSwap = false)
		{
			return IInventory.TryMove(this, toIndex, fromInv, fromIndex, allowSwap);
		}

		public void CopyFrom(ItemStack[] stacks) => CopyFrom(stacks.AsSpan());

		public void CopyFrom(ReadOnlySpan<ItemStack> stacks)
		{
			if (size < stacks.Length)
			{
				SetSize(stacks.Length);
			}

			for (int i = 0; i < stacks.Length; i++)
			{
				ref ItemStack slot = ref slots[i];
				ItemStack stack = stacks[i];

				if (slot != stack)
				{
					SetSlot(ref slot, i, stack);
				}
			}

			for (int i = stacks.Length; i < size; i++)
			{
				ref ItemStack slot = ref slots[i];

				if (slot != default)
				{
					SetSlot(ref slot, i, default);
				}
			}
		}

		public void CopyFrom(IReadOnlyList<ItemStack> stacks)
		{
			if (size < stacks.Count)
			{
				SetSize(stacks.Count);
			}

			for (int i = 0; i < stacks.Count; i++)
			{
				ref ItemStack slot = ref slots[i];
				ItemStack stack = stacks[i];

				if (slot != stack)
				{
					SetSlot(ref slot, i, stack);
				}
			}

			for (int i = stacks.Count; i < size; i++)
			{
				ref ItemStack slot = ref slots[i];

				if (slot != default)
				{
					SetSlot(ref slot, i, default);
				}
			}
		}

		public void Sort() => Sort(ValueComparer<ItemStack>.Default);

		public void Sort<TComparer>(TComparer comparer) where TComparer : IComparer<ItemStack>
		{
			ItemStack[] original = arrayPool.Rent(size);
			Array.Copy(slots, 0, original, 0, size);

			// We DON'T use the default Array.Sort because it is unstable. I think a stable sorting algorithm is very valuable here.
			// Our version also tends to be faster in general, so that's nice.
			TimSortLite.Sort(slots.AsSpan(0, size), comparer);

			for (int i = 0; i < size; i++)
			{
				ItemStack slot = slots[i];
				ItemStack originalSlot = original[i];

				if (slot != originalSlot)
				{
					OnSlotChanged(i, slot, originalSlot);
				}
			}

			Array.Clear(original, 0, size);
			arrayPool.Return(original);
		}

		public void Clear()
		{
			for (int i = 0; i < size; i++)
			{
				ref ItemStack slot = ref slots[i];

				if (slot != default)
				{
					SetSlot(ref slot, i, default);
				}
			}
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

		public ReadOnlySpan<ItemStack> AsSpan() => new(slots, 0, size);

		public ReadOnlySpan<ItemStack>.Enumerator GetEnumerator() => AsSpan().GetEnumerator();

		IEnumerator<ItemStack> IEnumerable<ItemStack>.GetEnumerator() => new ArrayEnumerator<ItemStack>(slots, size);
		IEnumerator IEnumerable.GetEnumerator() => new ArrayEnumerator<ItemStack>(slots, size);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void SetSlot(ref ItemStack slot, int index, in ItemStack stack)
		{
			if (slot.Count <= 0 && stack.Count > 0) occupiedSlots++;
			else if (slot.Count > 0 && stack.Count <= 0) occupiedSlots--;
			ItemStack previous = slot;
			slot = stack;
			OnSlotChanged(index, previous, slot);
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
				for (int i = value; i < size; i++)
				{
					ref ItemStack slot = ref slots[i];
					if (slot != default)
					{
						SetSlot(ref slot, i, default);
					}
				}
			}

			int previous = size;
			size = value;
			OnSizeChanged(previous, size);
		}
	}
}