namespace Kryz.RPG.Items
{
	public readonly struct ItemStack
	{
		public readonly IItem? Item;
		public readonly int Count;
		public readonly int MaxCount;

		public ItemStack(IItem item) : this(item, 1, item.Definition.MaxStacks) { }

		public ItemStack(IItem item, int count) : this(item, count, item.Definition.MaxStacks) { }

		public ItemStack(IItem item, int count, int maxCount)
		{
			if (item != null && count > 0)
			{
				Item = item;
				Count = count;
				MaxCount = maxCount;
			}
			else
			{
				Item = null;
				Count = 0;
				MaxCount = 0;
			}
		}

		public ItemStack Split(int count, out ItemStack splitStack)
		{
			TrySplit(count, out ItemStack stack, out splitStack);
			return stack;
		}

		public bool TrySplit(int count, out ItemStack a, out ItemStack b)
		{
			if (count >= 0 && count <= Count)
			{
				a = new ItemStack(Item!, Count - count);
				b = new ItemStack(Item!, count);
				return true;
			}
			a = this;
			b = default;
			return false;
		}
	}
}