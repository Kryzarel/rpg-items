using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kryz.RPG.Items.Interfaces;
using Kryz.RPG.Items.Structs;
using Kryz.Utils;

namespace Kryz.RPG.Items.Classes
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

		public Inventory(int numSlots, ArrayPool<ItemStack>? arrayPool = null)
		{
			this.arrayPool = arrayPool ?? ArrayPool<ItemStack>.Shared;
			slots = null!;
			SetSize(numSlots);
		}

		public Inventory(IReadOnlyList<ItemStack> items, ArrayPool<ItemStack>? arrayPool = null) : this(items.Count, arrayPool)
		{
			CopyFrom(items);
		}

		public bool CanSet(int index, in ItemStack stack)
		{
			return index >= 0 && index < size && (stack.Item == null || stack.Count <= stack.Item.MaxStacks);
		}

		public bool CanAdd(in ItemStack stack, out ItemStack remainder, bool allowPartial = false)
		{
			remainder = stack;

			if (stack.Item == null)
			{
				return false;
			}

			for (int i = 0; i < size && remainder.Count > 0; i++)
			{
				// Call ItemStack.TryAdd() but don't assign the result to anything.
				// This way we check what WOULD happen without actually changing the inventory.
				slots[i].TryAdd(remainder, out _, out remainder);
			}

			if (remainder.Count < stack.Count && (remainder.Count == 0 || allowPartial))
			{
				return true;
			}

			remainder = stack;
			return false;
		}

		public bool CanAdd(int index, in ItemStack stack, out ItemStack remainder, bool allowPartial = false)
		{
			if (stack.Item == null || index < 0 || index >= size)
			{
				remainder = stack;
				return false;
			}

			if (slots[index].TryAdd(stack, out _, out remainder) && (remainder.Count == 0 || allowPartial))
			{
				return true;
			}

			remainder = stack;
			return false;
		}

		public bool Contains(in ItemStack stack)
		{
			if (stack.Item == null)
			{
				return false;
			}

			for (int i = 0, required = stack.Count; i < size; i++)
			{
				ItemStack slot = slots[i];

				if (slot.Item == stack.Item)
				{
					required -= slot.Count;

					if (required <= 0)
					{
						return true;
					}
				}
			}

			return false;
		}

		public bool TrySet(int index, in ItemStack stack, bool forceSet = false)
		{
			if (index >= 0 && index < size && (forceSet || CanSet(index, stack)))
			{
				ref ItemStack slot = ref slots[index];

				if (slot != stack)
				{
					SetSlot(ref slot, index, stack);
					return true;
				}
			}

			return false;
		}

		public bool TryAdd(in ItemStack stack, out ItemStack remainder, bool allowPartial = false)
		{
			remainder = stack;

			if (stack.Item == null || !allowPartial && !CanAdd(stack, out _, allowPartial))
			{
				return false;
			}

			int maxCount = stack.Item.MaxStacks;

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

		public bool TryAdd(int index, in ItemStack stack, out ItemStack remainder, bool allowPartial = false)
		{
			if (stack.Item == null || index < 0 || index >= size)
			{
				remainder = stack;
				return false;
			}

			ref ItemStack slot = ref slots[index];

			if (slot.TryAdd(stack, out ItemStack result, out remainder) && (remainder.Count == 0 || allowPartial))
			{
				SetSlot(ref slot, index, result);
				return true;
			}

			remainder = stack;
			return false;
		}

		public bool TryRemove(in ItemStack stack)
		{
			if (!Contains(stack))
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

		public bool TryRemove(int index, int count, out ItemStack removed)
		{
			if (index < 0 || index >= size)
			{
				removed = default;
				return false;
			}

			return TryRemove(ref slots[index], index, count, out removed);
		}

		public bool TryRemove(int index, out ItemStack removed)
		{
			if (index < 0 || index >= size)
			{
				removed = default;
				return false;
			}

			ref ItemStack slot = ref slots[index];
			return TryRemove(ref slot, index, slot.Count, out removed);
		}

		public bool TryMove(int toIndex, int fromIndex, IInventory fromInv, bool allowSwap = false)
		{
			if (toIndex < 0 || toIndex >= size || fromIndex < 0 || fromIndex >= fromInv.Size || (toIndex == fromIndex && this == fromInv))
			{
				return false;
			}

			ItemStack fromSlot = fromInv[fromIndex];

			if (fromSlot.Item == null)
			{
				return false;
			}

			ref ItemStack toSlot = ref slots[toIndex];

			if (toSlot.TryAdd(fromSlot, out ItemStack result, out ItemStack remainder) && fromInv.TrySet(fromIndex, remainder))
			{
				SetSlot(ref toSlot, toIndex, result);
				return true;
			}
			else if (allowSwap && CanSet(toIndex, fromSlot) && fromInv.TrySet(fromIndex, toSlot))
			{
				SetSlot(ref toSlot, toIndex, fromSlot);
				return true;
			}

			return false;
		}

		public void Sort(IComparer<ItemStack>? comparer = null)
		{
			ItemStack[] original = arrayPool.Rent(size);
			Array.Copy(slots, 0, original, 0, size);

			// We DON'T use the default Array.Sort because it is unstable. I think a stable sorting algorithm is very valuable here.
			// Our version has similar speed for smaller arrays but tends to be faster on larger ones (around length=100) so that's also nice.
			MergeBinarySort.Sort(slots, 0, size, comparer ?? Comparer<ItemStack>.Default);

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

		public void Dispose()
		{
			Array.Clear(slots, 0, size);
			arrayPool.Return(slots);
		}

		public ArrayEnumerator<ItemStack> GetEnumerator() => new(slots, size);

		IEnumerator<ItemStack> IEnumerable<ItemStack>.GetEnumerator() => GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void SetSlot(ref ItemStack slot, int index, in ItemStack stack)
		{
			if (slot.Count <= 0 && stack.Count > 0) occupiedSlots++;
			else if (slot.Count > 0 && stack.Count <= 0) occupiedSlots--;
			ItemStack previous = slot;
			slot = stack;
			OnSlotChanged(index, previous, slot);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool TryRemove(ref ItemStack slot, int index, int count, out ItemStack removed)
		{
			if (slot.TryRemove(count, out ItemStack result, out removed))
			{
				SetSlot(ref slot, index, result);
				return true;
			}
			return false;
		}

		private void SetSize(int value)
		{
			if (value < 0) value = 0;
			if (value == size && slots != null) return;

			if (slots == null || value > slots.Length)
			{
				ItemStack[]? oldSlots = slots;
				// If 'value' is 0, ArrayPool SHOULD return Array.Empty<T>().
				slots = arrayPool.Rent(value);

				// We SHOULD NOT return the empty array to the pool.
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

			int previous = size;
			size = value;
			OnSizeChanged(previous, size);
		}
	}
}