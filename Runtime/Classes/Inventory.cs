using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
			filledSlots = 0;
		}

		public bool TryAdd(ItemStack stack, out ItemStack remainder)
		{
			if (stack.Item == null || stack.Count == 0)
			{
				remainder = stack;
				return false;
			}

			int startingCount = stack.Count;
			// If we have some modifier to the maximum stacks each inventory can hold, apply it here
			int maxCount = stack.Item.Definition.MaxStacks;
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
			ReadOnlySpan<int> freeSlots = AddOrGetFreeSlots(ref stack, maxCount, scope);

			// Success, we added the whole stack
			if (stack.Count == 0)
			{
				remainder = default;
				return true;
			}

			// If we still haven't added the whole stack, we try to add to free slots
			foreach (int index in freeSlots)
			{
				// Get the slot by reference so we can change it without having to reindex into the array
				ref ItemStack slot = ref slots[index];

				bool wasEmpty = slot.Count == 0;

				if (slot.TryAdd(stack, maxCount, out ItemStack result, out stack))
				{
					slot = result;
					if (wasEmpty) filledSlots++;
					OnSlotChanged(index);
				}

				// Success, we added the whole stack
				if (stack.Count == 0)
				{
					remainder = default;
					return true;
				}
			}

			remainder = stack;
			return remainder.Count < startingCount; // Return true if we added at least 1 item
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

				// How much to remove from this slot. We either remove the whole stack, or just enough to get what we need.
				int toRemove = Math.Min(required, slot.Count);
				slot = slot.Remove(toRemove, out ItemStack removedStack);
				if (slot.Count == 0) filledSlots--;

				Debug.Assert(toRemove == removedStack.Count);
				required -= toRemove;

				OnSlotChanged(index);
			}

			removed = new ItemStack(item, count);
			return true;
		}

		public bool TryRemove(int index, out ItemStack removed)
		{
			if (index < 0 || index >= numSlots)
			{
				removed = default;
				return false;
			}

			ref ItemStack slot = ref slots[index];
			if (slot.Item == null || slot.Count == 0)
			{
				removed = default;
				return false;
			}

			removed = slot;
			slot = default;
			filledSlots--;
			OnSlotChanged(index);
			return true;
		}

		public bool TryAdd(int index, ItemStack stack, out ItemStack remainder)
		{
			if (index < 0 || index >= numSlots || stack.Item == null || stack.Count == 0)
			{
				remainder = stack;
				return false;
			}

			int maxCount = stack.Item.Definition.MaxStacks;
			ref ItemStack slot = ref slots[index];

			bool wasEmpty = slot.Count == 0;

			if (slot.TryAdd(stack, maxCount, out ItemStack result, out stack))
			{
				slot = result;
				remainder = stack;
				if (wasEmpty) filledSlots++;
				OnSlotChanged(index);
				return true;
			}

			remainder = stack;
			return false;
		}

		public bool TryMove(int from, int to)
		{
			if (from < 0 || from >= numSlots || to < 0 || to >= numSlots)
			{
				return false;
			}

			ref ItemStack fromSlot = ref slots[from];
			if (fromSlot.Item == null || fromSlot.Count == 0)
			{
				return false;
			}

			ref ItemStack toSlot = ref slots[to];
			int maxCount = fromSlot.Item.Definition.MaxStacks;

			if (toSlot.TryAdd(fromSlot, maxCount, out ItemStack result, out ItemStack remainder))
			{
				fromSlot = remainder;
				toSlot = result;
				OnSlotChanged(from);
				OnSlotChanged(to);
				return true;
			}

			(fromSlot, toSlot) = (toSlot, fromSlot);
			OnSlotChanged(from);
			OnSlotChanged(to);
			return true;
		}

		public ArrayEnumerator<ItemStack> GetEnumerator() => new(slots, numSlots);

		IEnumerator<ItemStack> IEnumerable<ItemStack>.GetEnumerator() => GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ReadOnlySpan<int> AddOrGetFreeSlots(ref ItemStack stack, int maxCount, Span<int> freeSlots)
		{
			int freeCount = 0;

			for (int i = 0; i < numSlots; i++)
			{
				// Get the slot by reference so we can change it without having to reindex into the array
				ref ItemStack slot = ref slots[i];

				if (slot.Item == stack.Item)
				{
					bool wasEmpty = slot.Count == 0;

					if (slot.TryAdd(stack, maxCount, out ItemStack result, out stack))
					{
						slot = result;
						if (wasEmpty) filledSlots++;
						OnSlotChanged(i);
					}

					// Success, we added the whole stack
					if (stack.Count == 0)
					{
						return default;
					}
				}
				else if (freeCount < freeSlots.Length && slot.Item == null)
				{
					freeSlots[freeCount++] = i;
				}
			}

			return freeSlots[..freeCount];
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
		}
	}
}