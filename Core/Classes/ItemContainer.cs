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

		public virtual bool TryAdd(in ItemStack stack, out ItemStack remainder)
		{
			using ItemTransaction transaction = new(this);
			return transaction.SetResult(TryAdd(transaction, stack, out remainder));
		}

		public virtual bool TryAddSlot(int index, in ItemStack stack, out ItemStack result, out ItemStack remainder)
		{
			using ItemTransaction transaction = new(this);
			return transaction.SetResult(TryAddSlot(transaction, index, stack, out result, out remainder));
		}

		public virtual bool TryRemove(in ItemStack stack)
		{
			using ItemTransaction transaction = new(this);
			return transaction.SetResult(TryRemove(transaction, stack));
		}

		public virtual bool TryRemoveSlot(int index, out ItemStack result, out ItemStack removed, int count = -1)
		{
			using ItemTransaction transaction = new(this);
			return transaction.SetResult(TryRemoveSlot(transaction, index, out result, out removed, count));
		}

		public virtual bool TryMove(int fromIndex, IItemContainer to, int count = -1)
		{
			using ItemTransaction transaction = new(this);
			return transaction.SetResult(TryMove(transaction, fromIndex, to, count));
		}

		public virtual bool TryMoveSlot(int fromIndex, IItemContainer to, int toIndex, int count = -1)
		{
			using ItemTransaction transaction = new(this);
			return transaction.SetResult(TryMoveSlot(transaction, fromIndex, to, toIndex, count));
		}

		public virtual bool TrySetSize(int value)
		{
			using ItemTransaction transaction = new(this);
			return transaction.SetResult(TrySetSize(transaction, value));
		}

		public virtual void CopyFrom(ItemStack[] stacks) => CopyFrom(stacks.AsSpan());

		public virtual void CopyFrom(ReadOnlySpan<ItemStack> stacks)
		{
			using ItemTransaction transaction = new(this);
			int size = stacks.Length;

			SetSize(transaction, size);

			for (int i = 0; i < size; i++)
			{
				SetSlot(transaction, i, stacks[i]);
			}

			transaction.SetResult(true);
		}

		public virtual void CopyFrom(IReadOnlyList<ItemStack> stacks)
		{
			using ItemTransaction transaction = new(this);
			int size = stacks.Count;

			SetSize(transaction, size);

			for (int i = 0; i < size; i++)
			{
				SetSlot(transaction, i, stacks[i]);
			}

			transaction.SetResult(true);
		}

		public virtual void Clear()
		{
			using ItemTransaction transaction = new(this);
			for (int i = 0, size = Size; i < size; i++)
			{
				SetSlot(transaction, i, default);
			}
			transaction.SetResult(true);
		}

		public virtual void Sort()
		{
			using ItemTransaction transaction = new(this);
			Sort(transaction);
			transaction.SetResult(true);
		}
	}
}