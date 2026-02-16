using System;
using System.Buffers;
using System.Collections.Generic;

namespace Kryz.RPG.Items
{
	public abstract class ItemContainerBase : ItemContainerTransactionalBase, IItemContainer
	{
		public ItemContainerBase(int size, ArrayPool<ItemStack>? pool = null) : base(size, pool)
		{
		}

		public virtual bool TryAdd(in ItemStack stack, out ItemStack remainder)
		{
			using ItemTransaction transaction = new(this);
			if (!TryAdd(transaction, stack, out remainder))
			{
				return false;
			}
			transaction.Commit();
			return true;
		}

		public virtual bool TryAddSlot(int index, in ItemStack stack, out ItemStack result, out ItemStack remainder)
		{
			using ItemTransaction transaction = new(this);
			if (!TryAddSlot(transaction, index, stack, out result, out remainder))
			{
				return false;
			}
			transaction.Commit();
			return true;
		}

		public virtual bool TryRemove(in ItemStack stack)
		{
			using ItemTransaction transaction = new(this);
			if (!TryRemove(transaction, stack))
			{
				return false;
			}
			transaction.Commit();
			return true;
		}

		public virtual bool TryRemoveSlot(int index, out ItemStack result, out ItemStack removed, int count = -1)
		{
			using ItemTransaction transaction = new(this);
			if (!TryRemoveSlot(transaction, index, out result, out removed, count))
			{
				return false;
			}
			transaction.Commit();
			return true;
		}

		public virtual bool CanMove(int fromIndex, IItemContainer to, int count = -1)
		{
			using ItemTransaction transaction = new(this);
			return IItemContainer.TryMove(transaction, this, fromIndex, to, count);
		}

		public virtual bool TryMove(int fromIndex, IItemContainer to, int count = -1)
		{
			using ItemTransaction transaction = new(this);
			if (!IItemContainer.TryMove(transaction, this, fromIndex, to, count))
			{
				return false;
			}
			transaction.Commit();
			return true;
		}

		public virtual bool CanMoveSlot(int fromIndex, IItemContainer to, int toIndex, int count = -1)
		{
			using ItemTransaction transaction = new(this);
			return IItemContainer.TryMoveSlot(transaction, this, fromIndex, to, toIndex, count);
		}

		public virtual bool TryMoveSlot(int fromIndex, IItemContainer to, int toIndex, int count = -1)
		{
			using ItemTransaction transaction = new(this);
			if (!IItemContainer.TryMoveSlot(transaction, this, fromIndex, to, toIndex, count))
			{
				return false;
			}
			transaction.Commit();
			return true;
		}

		public virtual bool TrySetSize(int value)
		{
			using ItemTransaction transaction = new(this);
			if (!TrySetSize(transaction, value))
			{
				return false;
			}
			transaction.Commit();
			return true;
		}

		public virtual void CopyFrom(ItemStack[] stacks) => CopyFrom(stacks.AsSpan());

		public virtual void CopyFrom(ReadOnlySpan<ItemStack> stacks)
		{
			using ItemTransaction transaction = new(this);
			int size = stacks.Length;

			if (Size < size)
			{
				TrySetSize(transaction, size);
			}

			for (int i = 0; i < size; i++)
			{
				ItemStack stack = stacks[i];
				SetSlot(transaction, i, stack);
			}

			transaction.Commit();
		}

		public virtual void CopyFrom(IReadOnlyList<ItemStack> stacks)
		{
			using ItemTransaction transaction = new(this);
			int size = stacks.Count;

			if (Size < size)
			{
				TrySetSize(transaction, size);
			}

			for (int i = 0; i < size; i++)
			{
				ItemStack stack = stacks[i];
				SetSlot(transaction, i, stack);
			}

			transaction.Commit();
		}

		public virtual void Clear()
		{
			using ItemTransaction transaction = new(this);
			for (int i = 0, size = Size; i < size; i++)
			{
				SetSlot(transaction, i, default);
			}
			transaction.Commit();
		}

		public virtual void Sort()
		{
			using ItemTransaction transaction = new(this);
			Sort(transaction);
			transaction.Commit();
		}
	}
}