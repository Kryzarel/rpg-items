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
	public class Inventory : IInventory
	{
		private readonly ArrayPool<ItemStack> arrayPool;

		private ItemStack[] slots;
		private int numSlots;
		private int filledSlots;

		public event Action<int> OnSlotChanged = delegate { };
		public event Action<int> OnNumSlotsChanged = delegate { };

		public ItemStack this[int index] => slots[index];
		public int Count => numSlots;
		public int NumSlots { get => numSlots; set => SetNumSlots(value); }
		public int FreeSlots => numSlots - filledSlots;
		public int FilledSlots => filledSlots;

		public Inventory(int numSlots, ArrayPool<ItemStack>? arrayPool = null)
		{
			this.arrayPool = arrayPool ?? ArrayPool<ItemStack>.Shared;
			slots = null!;
			SetNumSlots(numSlots);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetMaxStackSize(IItem item)
		{
			// If we have some modifier to the maximum stacks each inventory can hold, apply it here
			return item.Definition.MaxStacks;
		}

		public bool CanAdd(in ItemStack stack, out ItemStack remainder)
		{
			if (stack.Item == null || stack.Count == 0)
			{
				remainder = default;
				return false;
			}

			remainder = stack;
			int maxCount = GetMaxStackSize(stack.Item);

			for (int i = 0; i < numSlots && remainder.Count > 0; i++)
			{
				// Call ItemStack.TryAdd() but don't assign the result to anything.
				// This way we check what WOULD happen without actually changing the inventory.
				slots[i].TryAdd(remainder, maxCount, out _, out remainder);
			}

			return remainder.Count < stack.Count;
		}

		public bool CanAdd(int index, in ItemStack stack, out ItemStack remainder)
		{
			if (stack.Item == null || stack.Count == 0)
			{
				remainder = default;
				return false;
			}

			int maxCount = GetMaxStackSize(stack.Item);
			return slots[index].TryAdd(stack, maxCount, out _, out remainder);
		}

		public bool CanRemove(IItem item, int count)
		{
			for (int i = 0; i < numSlots && count > 0; i++)
			{
				ItemStack slot = slots[i];
				if (slot.Item == item)
				{
					count -= slot.Count;
				}
			}
			return count <= 0;
		}

		public bool CanRemove(int index, int count)
		{
			return slots[index].Count >= count;
		}

		public bool CanRemove(int index)
		{
			return slots[index].Count > 0;
		}

		public bool CanSwap(int index, in ItemStack stack, out ItemStack removed)
		{
			ItemStack slot = slots[index];
			bool success = stack != slot && (stack.Item == null || stack.Count <= GetMaxStackSize(stack.Item));
			removed = success ? slot : default;
			return success;
		}

		public bool TryAdd(in ItemStack stack, out ItemStack remainder)
		{
			remainder = stack;

			if (stack.Item == null || stack.Count == 0)
			{
				return false;
			}

			int maxCount = GetMaxStackSize(stack.Item);
			// If the stack has more items than 1 free slot can take, we need multiple free slots
			// Fast integer division that always rounds up: https://stackoverflow.com/a/4175152
			int freeSlotsNeeded = (stack.Count + maxCount - 1) / maxCount;
			// There's no point requesting more free slots than the inventory has available
			freeSlotsNeeded = Math.Min(freeSlotsNeeded, FreeSlots);

			// If we need less than 1024 slots, we stackalloc an array of ints, otherwise we get an array from the pool to prevent a stack overflow
			// Using 'SpanScope' shortens this process to a single line, rather than having to manually manage pooled arrays
			// The likelyhood that we'll ever need more than 1024 slots for a single item stack is pretty small, but you never know
			using SpanScope<int> scope = freeSlotsNeeded > 1024 ? new SpanScope<int>(freeSlotsNeeded) : stackalloc int[freeSlotsNeeded];

			// First we try to add to slots that already have this item
			ReadOnlySpan<int> freeSlots = AddOrGetFreeSlots(remainder, maxCount, scope, out remainder);

			// If we still haven't added the whole stack, we try to add to free slots
			for (int i = 0; i < freeSlots.Length && remainder.Count > 0; i++)
			{
				int index = freeSlots[i];
				ref ItemStack slot = ref slots[index]; // Get the slot by reference so we can change it without having to reindex into the array

				if (slot.TryAdd(remainder, maxCount, out ItemStack result, out remainder))
				{
					SetSlot(ref slot, index, result);
				}
			}

			// AddToSlotsWithItem(remainder, stack.Item, maxCount, out remainder);
			// AddToSlotsWithItem(remainder, null, maxCount, out remainder);

			return remainder.Count < stack.Count; // Return true if we added at least 1 item
		}

		// private void AddToSlotsWithItem(in ItemStack stack, IItem? item, int maxCount, out ItemStack remainder)
		// {
		// 	remainder = stack;

		// 	for (int i = 0; i < numSlots && remainder.Count > 0; i++)
		// 	{
		// 		ref ItemStack slot = ref slots[i];

		// 		if (slot.Item == item && slot.TryAdd(remainder, maxCount, out ItemStack result, out remainder))
		// 		{
		// 			SetSlot(ref slot, i, result);
		// 		}
		// 	}
		// }

		public bool TryAdd(int index, in ItemStack stack, out ItemStack remainder)
		{
			if (stack.Item == null || stack.Count == 0)
			{
				remainder = stack;
				return false;
			}

			ref ItemStack slot = ref slots[index];
			int maxCount = GetMaxStackSize(stack.Item);

			if (slot.TryAdd(stack, maxCount, out ItemStack result, out remainder))
			{
				SetSlot(ref slot, index, result);
				return true;
			}

			return false;
		}

		public bool TryRemove(IItem item, int count, out ItemStack removed)
		{
			if (item == null || count <= 0)
			{
				removed = default;
				return false;
			}

			// First we check if there's enough of the requested item
			// We save the slot indexes that have the item so that we can go back to them faster when removing
			using SpanScope<int> scope = numSlots > 1024 ? new SpanScope<int>(numSlots) : stackalloc int[numSlots];
			ReadOnlySpan<int> slotsWithItem = GetSlotsWithItem(item, scope, out int availableCount, count);

			// We don't have enough of the requested item
			if (availableCount < count)
			{
				removed = default;
				return false;
			}

			for (int i = 0, required = count; i < slotsWithItem.Length && required > 0; i++)
			{
				int index = slotsWithItem[i];
				ref ItemStack slot = ref slots[index];
				int countToRemove = Math.Min(required, slot.Count);

				if (slot.TryRemove(countToRemove, out ItemStack result, out ItemStack removedStack))
				{
					SetSlot(ref slot, index, result);
					required -= removedStack.Count;
				}
			}

			removed = new ItemStack(item, count);
			return true;
		}

		public bool TryRemove(int index, int count, out ItemStack removed)
		{
			return TryRemove(ref slots[index], index, count, out removed);
		}

		public bool TryRemove(int index, out ItemStack removed)
		{
			ref ItemStack slot = ref slots[index];
			return TryRemove(ref slot, index, slot.Count, out removed);
		}

		public bool TrySwap(int index, in ItemStack stack, out ItemStack removed)
		{
			ref ItemStack slot = ref slots[index];

			if (stack != slot && (stack.Item == null || stack.Count <= GetMaxStackSize(stack.Item)))
			{
				removed = slot;
				SetSlot(ref slot, index, stack);
				return true;
			}

			removed = default;
			return false;
		}

		public bool TryMove(int toIndex, int fromIndex, IInventory fromInventory, bool allowSwap = false)
		{
			ItemStack fromSlot = fromInventory[fromIndex];

			if (toIndex == fromIndex && this == fromInventory || fromSlot.Item == null)
			{
				return false;
			}

			ref ItemStack toSlot = ref slots[toIndex];
			int maxCount = GetMaxStackSize(fromSlot.Item);

			if (toSlot.TryAdd(fromSlot, maxCount, out ItemStack result, out ItemStack remainder) && fromInventory.TrySwap(fromIndex, remainder, out _))
			{
				SetSlot(ref toSlot, toIndex, result);
				return true;
			}
			else if (allowSwap && fromSlot.Count <= maxCount && fromInventory.TrySwap(fromIndex, toSlot, out _))
			{
				SetSlot(ref toSlot, toIndex, fromSlot);
				return true;
			}
			return false;
		}

		public ArrayEnumerator<ItemStack> GetEnumerator() => new(slots, numSlots);

		IEnumerator<ItemStack> IEnumerable<ItemStack>.GetEnumerator() => GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void SetSlot(ref ItemStack slot, int index, in ItemStack stack)
		{
			if (slot.Count <= 0 && stack.Count > 0) filledSlots++;
			else if (slot.Count > 0 && stack.Count <= 0) filledSlots--;
			slot = stack;
			OnSlotChanged(index);
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ReadOnlySpan<int> AddOrGetFreeSlots(in ItemStack stack, int maxCount, Span<int> freeSlots, out ItemStack remainder)
		{
			int freeCount = 0;
			remainder = stack;

			for (int i = 0; i < numSlots && remainder.Count > 0; i++)
			{
				// Get the slot by reference so we can change it without having to reindex into the array
				ref ItemStack slot = ref slots[i];

				if (slot.Item == stack.Item)
				{
					if (slot.TryAdd(remainder, maxCount, out ItemStack result, out remainder))
					{
						SetSlot(ref slot, i, result);
					}
				}
				else if (slot.Item == null && freeCount < freeSlots.Length)
				{
					freeSlots[freeCount++] = i;
				}
			}

			// Return free slots or an empty span if we managed to add the whole stack
			return remainder.Count > 0 ? freeSlots[..freeCount] : default;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ReadOnlySpan<int> GetSlotsWithItem(IItem item, Span<int> slotIndexes, out int availableCount, int requiredCount = int.MaxValue)
		{
			availableCount = 0;
			int slotCount = 0;

			for (int i = 0; i < numSlots && availableCount < requiredCount; i++)
			{
				ItemStack slot = slots[i];

				if (slot.Item == item)
				{
					availableCount += slot.Count;
					slotIndexes[slotCount++] = i;
				}
			}

			return slotIndexes[..slotCount];
		}

		private void SetNumSlots(int value)
		{
			if (numSlots == value && slots != null)
			{
				return;
			}

			if (value <= 0)
			{
				value = 0;

				if (slots != null)
				{
					Array.Clear(slots, 0, numSlots);
					arrayPool.Return(slots);
				}

				slots = Array.Empty<ItemStack>();
			}
			else if (slots == null || value > slots.Length)
			{
				ItemStack[] newSlots = arrayPool.Rent(value);

				if (slots != null && slots.Length > 0)
				{
					Array.Copy(slots, newSlots, numSlots);
					Array.Clear(slots, 0, numSlots);
					arrayPool.Return(slots);
				}

				slots = newSlots;
			}

			numSlots = value;
			OnNumSlotsChanged(value);
		}
	}
}