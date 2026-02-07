using System;

namespace Kryz.RPG.Items
{
	public abstract class BaseItemContainerStrategy : IItemContainerStrategy
	{
		public virtual int GetMaxStacks(IItem item) => item.MaxStacks;

		public virtual bool CanReceive(int index, in ItemStack stack) => true;

		public virtual bool CanRemove(int index) => true;

		public virtual bool TryAdd(ref ItemContainerTransactionContext ctx, in ItemStack stack, out ItemStack remainder)
		{
			remainder = stack;

			if (stack.Item == null)
			{
				return false;
			}

			int maxCount = GetMaxStacks(stack.Item);
			int size = ctx.GetSize();

			// First, try adding to slots that already have the same item
			for (int i = 0; i < size && remainder.Count > 0; i++)
			{
				ItemStack slot = ctx.GetSlot(i);

				if (slot.Item == stack.Item && CanReceive(i, stack) && slot.TryAdd(stack, out ItemStack result, out remainder, maxCount))
				{
					ctx.SetSlot(i, result);
				}
			}

			// Then, try adding to empty slots
			for (int i = 0; i < size && remainder.Count > 0; i++)
			{
				ItemStack slot = ctx.GetSlot(i);

				if (slot.Item == null && CanReceive(i, stack) && slot.TryAdd(stack, out ItemStack result, out remainder, maxCount))
				{
					ctx.SetSlot(i, result);
				}
			}

			return remainder.Count < stack.Count; // Return true if we added at least 1 item
		}

		public virtual bool TryAdd(ref ItemContainerTransactionContext ctx, int index, in ItemStack stack, out ItemStack result, out ItemStack remainder)
		{
			ItemStack slot = ctx.GetSlot(index);

			if (stack.Item != null && CanReceive(index, stack) && slot.TryAdd(stack, out result, out remainder, GetMaxStacks(stack.Item)))
			{
				ctx.SetSlot(index, result);
				return true;
			}

			remainder = stack;
			result = slot;
			return false;
		}

		public virtual bool TryRemove(ref ItemContainerTransactionContext ctx, in ItemStack stack)
		{
			if (stack.Item == null)
			{
				return false;
			}

			int size = ctx.GetSize();

			for (int i = 0, required = stack.Count; i < size; i++)
			{
				ItemStack slot = ctx.GetSlot(i);

				if (slot.Item == stack.Item && CanRemove(i) && slot.TryRemove(Math.Min(required, slot.Count), out ItemStack result, out ItemStack removed))
				{
					ctx.SetSlot(i, result);
					required -= removed.Count;

					if (required <= 0)
					{
						return true;
					}
				}
			}

			return false;
		}

		public virtual bool TryRemove(ref ItemContainerTransactionContext ctx, int index, int count, out ItemStack result, out ItemStack removed)
		{
			ItemStack slot = ctx.GetSlot(index);
			if (count < 0) count = slot.Count;

			if (CanRemove(index) && slot.TryRemove(count, out result, out removed))
			{
				ctx.SetSlot(index, result);
				return true;
			}

			removed = default;
			result = slot;
			return false;
		}
	}
}