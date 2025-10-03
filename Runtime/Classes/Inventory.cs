using System;
using System.Buffers;
using Kryz.RPG.Items.Interfaces;
using Kryz.RPG.Items.Structs;

namespace Kryz.RPG.Items
{
	public class Inventory : IInventory
	{
		private readonly ArrayPool<ItemStack> arrayPool;

		private int numSlots;
		private ItemStack[] slots;

		public int NumSlots { get => numSlots; set => slots = GetSlots(value); }

		public ReadOnlySpan<ItemStack> Slots => new(slots, 0, numSlots);

		public Inventory(int numSlots, ArrayPool<ItemStack>? arrayPool = null)
		{
			this.arrayPool = arrayPool ?? ArrayPool<ItemStack>.Shared;
			slots = GetSlots(numSlots);
		}

		public bool TryAdd(ItemStack stack, out ItemStack remainder)
		{
			if (stack.Item == null || stack.Count == 0)
			{
				remainder = stack;
				return false;
			}

			int startingCount = stack.Count;
			// We might have some modifier to the maximum stacks each inventory can hold
			int maxCount = stack.Item.Definition.MaxStacks;
			// If the stack has more items than 1 free slot can take, we need multiple free slots
			// Fast integer division that always rounds up: https://stackoverflow.com/a/4175152
			int freeSlotsNeeded = (stack.Count + maxCount - 1) / maxCount;

			// First we try to add to slots that already have this item
			ReadOnlySpan<int> freeSlots = AddOrGetFreeSlots(ref stack, maxCount, stackalloc int[freeSlotsNeeded]);

			// Success, we added the whole stack
			if (stack.Count == 0)
			{
				remainder = default;
				return true;
			}

			// If we still haven't added the whole stack, we try to add to free slots
			foreach (int index in freeSlots)
			{
				ref ItemStack slot = ref slots[index];
				slot = slot.Add(stack, maxCount, out stack);

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

		public bool TryAdd(int index, ItemStack stack, out ItemStack remainder)
		{
			if (index < 0 || index >= numSlots || stack.Item == null || stack.Count == 0)
			{
				remainder = stack;
				return false;
			}

			int maxCount = stack.Item.Definition.MaxStacks;
			ref ItemStack slot = ref slots[index];

			if (slot.TryAdd(stack, maxCount, out ItemStack result, out stack))
			{
				slot = result;
				remainder = stack;
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
				return true;
			}

			(fromSlot, toSlot) = (toSlot, fromSlot);
			return true;
		}

		public bool TryRemove(int index, out ItemStack stack)
		{
			if (index < 0 || index >= numSlots)
			{
				stack = default;
				return false;
			}

			ref ItemStack slot = ref slots[index];
			if (slot.Item == null || slot.Count == 0)
			{
				stack = default;
				return false;
			}

			stack = slot;
			slot = default;
			return true;
		}

		private ReadOnlySpan<int> AddOrGetFreeSlots(ref ItemStack stack, int maxCount, Span<int> freeBuffer)
		{
			int freeCount = 0;

			for (int i = 0; i < numSlots; i++)
			{
				ref ItemStack slot = ref slots[i];

				if (slot.Item == stack.Item)
				{
					slot = slot.Add(stack, maxCount, out stack);

					// Success, we added the whole stack
					if (stack == default)
					{
						return default;
					}
				}
				else if (freeCount < freeBuffer.Length && slot.Item == null)
				{
					freeBuffer[freeCount++] = i;
				}
			}

			return freeBuffer[..freeCount];
		}

		private ItemStack[] GetSlots(int value)
		{
			if (slots.Length < value)
			{
				if (slots != null)
				{
					Array.Clear(slots, 0, value);
					arrayPool.Return(slots);
				}
				slots = arrayPool.Rent(value);
				Array.Clear(slots, 0, slots.Length);
			}

			numSlots = value;
			return slots;
		}
	}
}