using System;
using System.Buffers;
using System.Collections.Generic;

namespace Kryz.RPG.Items
{
	public abstract class ItemContainer : ItemContainerTransactional, IItemContainer
	{
		public ItemContainer(int size, ArrayPool<ItemStack>? pool = null) : base(size, pool)
		{
		}

		public bool CanAdd(in ItemStack stack, out ItemStack remainder)
		{
			using ItemTransaction transaction = new(this);
			return TryAdd(transaction, stack, out remainder);
		}

		public bool CanAddSlot(int index, in ItemStack stack, out ItemStack result, out ItemStack remainder)
		{
			using ItemTransaction transaction = new(this);
			return TryAddSlot(transaction, index, stack, out result, out remainder);
		}

		public bool CanRemove(in ItemStack stack)
		{
			using ItemTransaction transaction = new(this);
			return TryRemove(transaction, stack);
		}

		public bool CanRemoveSlot(int index, out ItemStack result, out ItemStack removed, int count = -1)
		{
			using ItemTransaction transaction = new(this);
			return TryRemoveSlot(transaction, index, out result, out removed, count);
		}

		public bool CanMove(int fromIndex, IItemContainer to, int count = -1)
		{
			using ItemTransaction transaction = new(this);
			return TryMove(transaction, fromIndex, to, count);
		}

		public bool CanMoveSlot(int fromIndex, IItemContainer to, int toIndex, int count = -1)
		{
			using ItemTransaction transaction = new(this);
			return TryMove(transaction, fromIndex, to, count);
		}

		public bool CanSetSlot(int index, in ItemStack stack)
		{
			using ItemTransaction transaction = new(this);
			return TrySetSlot(transaction, index, stack);
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

		public virtual bool TryMove(int fromIndex, IItemContainer to, int count = -1)
		{
			using ItemTransaction transaction = new(this);
			if (!TryMove(transaction, fromIndex, to, count))
			{
				return false;
			}
			transaction.Commit();
			return true;
		}

		public virtual bool TryMoveSlot(int fromIndex, IItemContainer to, int toIndex, int count = -1)
		{
			using ItemTransaction transaction = new(this);
			if (!TryMoveSlot(transaction, fromIndex, to, toIndex, count))
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