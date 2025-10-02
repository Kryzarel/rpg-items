using System;

namespace Kryz.RPG.Items
{
	public readonly struct ItemStack
	{
		public readonly IItem? Item;
		public readonly int Count;

		public bool IsValid => Item != null && Count > 0;

		public ItemStack(IItem item) : this(item, 1) { }

		public ItemStack(IItem item, int count)
		{
			if (item != null && count > 0)
			{
				Item = item;
				Count = count;
			}
			else
			{
				Item = null;
				Count = 0;
			}
		}

		public ItemStack Add(ItemStack toAdd, int maxCount, out ItemStack remainder)
		{
			TryAdd(toAdd, maxCount, out ItemStack stack, out remainder);
			return stack;
		}

		public ItemStack Remove(int count, out ItemStack removedStack)
		{
			TryRemove(count, out ItemStack stack, out removedStack);
			return stack;
		}

		public bool TryAdd(ItemStack toAdd, int maxCount, out ItemStack result, out ItemStack remainder)
		{
			if (toAdd.Count > 0 && toAdd.Item != null && toAdd.Item == Item)
			{
				int newCount = Count + toAdd.Count;
				int remainingCount = newCount - maxCount;
				int resultCount = remainingCount > 0 ? maxCount : newCount;

				result = new ItemStack(Item, resultCount);
				remainder = new ItemStack(Item, remainingCount);
				return true;
			}

			result = this;
			remainder = default;
			return false;
		}

		public bool TryRemove(int count, out ItemStack result, out ItemStack removedStack)
		{
			if (count > 0 && count <= Count && Item != null)
			{
				result = new ItemStack(Item, Count - count);
				removedStack = new ItemStack(Item, count);
				return true;
			}

			result = this;
			removedStack = default;
			return false;
		}
	}
}