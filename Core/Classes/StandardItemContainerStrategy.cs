using System;

namespace Kryz.RPG.Items
{
	public class StandardItemContainerStrategy : IItemContainerStrategy
	{
		public int GetMaxStacks(IItem item) => item.MaxStacks;

		public bool TryAdd(Span<ItemStack> slots, in ItemStack stack, out ItemStack remainder)
		{
			remainder = stack;

			if (stack.Item == null)
			{
				return false;
			}

			int maxCount = GetMaxStacks(stack.Item);

			// First, try adding to slots that already have the same item
			for (int i = 0; i < slots.Length && remainder.Count > 0; i++)
			{
				if (slots[i].Item == stack.Item)
				{
					TryAdd(slots, i, remainder, out remainder, maxCount);
				}
			}

			// Then, try adding to empty slots
			for (int i = 0; i < slots.Length && remainder.Count > 0; i++)
			{
				if (slots[i].Item == null)
				{
					TryAdd(slots, i, remainder, out remainder, maxCount);
				}
			}

			return remainder.Count < stack.Count; // Return true if we added at least 1 item
		}

		public bool TryAdd(Span<ItemStack> slots, int index, in ItemStack stack, out ItemStack remainder)
		{
			remainder = stack;
			return stack.Item != null && TryAdd(slots, index, stack, out remainder, GetMaxStacks(stack.Item));
		}

		public bool TryAdd(Span<ItemStack> slots, int index, in ItemStack stack, out ItemStack remainder, int maxCount)
		{
			ref ItemStack slot = ref slots[index];

			if (slot.TryAdd(stack, out ItemStack result, out remainder, maxCount))
			{
				slot = result;
				return true;
			}

			remainder = slot;
			return false;
		}

		public bool TryRemove(Span<ItemStack> slots, in ItemStack stack)
		{
			if (stack.Item == null)
			{
				return false;
			}

			for (int i = 0, required = stack.Count; i < slots.Length; i++)
			{
				ItemStack slot = slots[i];

				if (slot.Item == stack.Item && TryRemove(slots, i, Math.Min(required, slot.Count), out ItemStack removed))
				{
					required -= removed.Count;

					if (required <= 0)
					{
						return true;
					}
				}
			}

			return false;
		}

		public bool TryRemove(Span<ItemStack> slots, int index, out ItemStack removed)
		{
			ref ItemStack slot = ref slots[index];

			if (slot.TryRemove(slot.Count, out ItemStack result, out removed))
			{
				slot = result;
				return true;
			}
			return false;
		}

		public bool TryRemove(Span<ItemStack> slots, int index, int count, out ItemStack removed)
		{
			ref ItemStack slot = ref slots[index];

			if (slot.TryRemove(count, out ItemStack result, out removed))
			{
				slot = result;
				return true;
			}
			return false;
		}

		public bool TrySet(Span<ItemStack> slots, int index, in ItemStack stack, out ItemStack previous)
		{
			ref ItemStack slot = ref slots[index];

			if (slot.Item == null || slot.Count <= GetMaxStacks(slot.Item))
			{
				previous = slot;
				slot = stack;
				return true;
			}

			previous = default;
			return false;
		}
	}
}