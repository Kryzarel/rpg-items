using System;
using System.Collections;
using System.Collections.Generic;

namespace Kryz.RPG.Items
{
	public abstract class ItemContainerBase<T> : IItemContainer where T : IItemContainerTransactional, new()
	{
		protected readonly T transactions;

		public T Transactions => transactions;

		public ItemStack this[int index] => transactions[index];
		public ReadOnlySpan<ItemStack> Slots => transactions.Slots;
		public int Size => transactions.Size;
		public int FreeSlots => transactions.FreeSlots;
		public int OccupiedSlots => transactions.OccupiedSlots;

		IItemContainerTransactional IItemContainer.Transactions => Transactions;
		int IReadOnlyCollection<ItemStack>.Count => transactions.Size;

		/// <summary>
		/// Even though 'new T()' is extremely convenient, it actually uses reflection at runtime - it calls <see cref="Activator.CreateInstance{T}"/>.
		/// Consider using the other constructor and manually instantiating the <see cref="IItemContainerTransactional"/> if that's a concern.
		/// </summary>
		/// <param name="size">The initial size of the internal <see cref="IItemContainerTransactional"/>.</param>
		public ItemContainerBase(int size)
		{
			transactions = new T();
			TrySetSize(size);
		}

		public ItemContainerBase(T itemContainerTransactional)
		{
			transactions = itemContainerTransactional;
		}

		public bool CanAdd(in ItemStack stack, out ItemStack remainder)
		{
			using ItemTransaction transaction = new(transactions);
			return transactions.TryAdd(transaction, stack, out remainder);
		}

		public bool TryAdd(in ItemStack stack, out ItemStack remainder)
		{
			using ItemTransaction transaction = new(transactions);
			if (!transactions.TryAdd(transaction, stack, out remainder))
			{
				return false;
			}
			transaction.Commit();
			return true;
		}

		public bool CanAddSlot(int index, in ItemStack stack, out ItemStack result, out ItemStack remainder)
		{
			using ItemTransaction transaction = new(transactions);
			return transactions.TryAddSlot(transaction, index, stack, out result, out remainder);
		}

		public bool TryAddSlot(int index, in ItemStack stack, out ItemStack result, out ItemStack remainder)
		{
			using ItemTransaction transaction = new(transactions);
			if (!transactions.TryAddSlot(transaction, index, stack, out result, out remainder))
			{
				return false;
			}
			transaction.Commit();
			return true;
		}

		public bool CanRemove(in ItemStack stack)
		{
			using ItemTransaction transaction = new(transactions);
			return transactions.TryRemove(transaction, stack);
		}

		public bool TryRemove(in ItemStack stack)
		{
			using ItemTransaction transaction = new(transactions);
			if (!transactions.TryRemove(transaction, stack))
			{
				return false;
			}
			transaction.Commit();
			return true;
		}

		public bool CanRemoveSlot(int index, out ItemStack result, out ItemStack removed, int count = -1)
		{
			using ItemTransaction transaction = new(transactions);
			return transactions.TryRemoveSlot(transaction, index, out result, out removed, count);
		}

		public bool TryRemoveSlot(int index, out ItemStack result, out ItemStack removed, int count = -1)
		{
			using ItemTransaction transaction = new(transactions);
			if (!transactions.TryRemoveSlot(transaction, index, out result, out removed, count))
			{
				return false;
			}
			transaction.Commit();
			return true;
		}

		public bool CanMove(int fromIndex, IItemContainer to, int count = -1)
		{
			using ItemTransaction transaction = new(transactions);
			return IItemContainer.TryMove(transaction, this, fromIndex, to, count);
		}

		public bool TryMove(int fromIndex, IItemContainer to, int count = -1)
		{
			using ItemTransaction transaction = new(transactions);
			if (!IItemContainer.TryMove(transaction, this, fromIndex, to, count))
			{
				return false;
			}
			transaction.Commit();
			return true;
		}

		public bool CanMoveSlot(int fromIndex, IItemContainer to, int toIndex, int count = -1)
		{
			using ItemTransaction transaction = new(transactions);
			return IItemContainer.TryMoveSlot(transaction, this, fromIndex, to, toIndex, count);
		}

		public bool TryMoveSlot(int fromIndex, IItemContainer to, int toIndex, int count = -1)
		{
			using ItemTransaction transaction = new(transactions);
			if (!IItemContainer.TryMoveSlot(transaction, this, fromIndex, to, toIndex, count))
			{
				return false;
			}
			transaction.Commit();
			return true;
		}

		public bool CanSetSize(int value)
		{
			using ItemTransaction transaction = new(transactions);
			return transactions.TrySetSize(transaction, value);
		}

		public bool TrySetSize(int value)
		{
			using ItemTransaction transaction = new(transactions);
			if (!transactions.TrySetSize(transaction, value))
			{
				return false;
			}
			transaction.Commit();
			return true;
		}

		public void CopyFrom(ItemStack[] stacks) => CopyFrom(stacks.AsSpan());

		public void CopyFrom(ReadOnlySpan<ItemStack> stacks)
		{
			using ItemTransaction transaction = new(transactions);
			int size = stacks.Length;

			if (transactions.Size < size)
			{
				transactions.TrySetSize(transaction, size);
			}

			for (int i = 0; i < size; i++)
			{
				ItemStack stack = stacks[i];
				transactions.SetSlot(transaction, i, stack);
			}

			transaction.Commit();
		}

		public void CopyFrom(IReadOnlyList<ItemStack> stacks)
		{
			using ItemTransaction transaction = new(transactions);
			int size = stacks.Count;

			if (transactions.Size < size)
			{
				transactions.TrySetSize(transaction, size);
			}

			for (int i = 0; i < size; i++)
			{
				ItemStack stack = stacks[i];
				transactions.SetSlot(transaction, i, stack);
			}

			transaction.Commit();
		}

		public void Clear()
		{
			using ItemTransaction transaction = new(transactions);
			for (int i = 0, size = transactions.Size; i < size; i++)
			{
				transactions.SetSlot(transaction, i, default);
			}
			transaction.Commit();
		}

		public void Sort()
		{
			using ItemTransaction transaction = new(transactions);
			transactions.Sort(transaction);
			transaction.Commit();
		}

		public void Sort<TComparer>(TComparer comparer) where TComparer : IComparer<ItemStack>
		{
			using ItemTransaction transaction = new(transactions);
			transactions.Sort(transaction, comparer);
			transaction.Commit();
		}

		public ReadOnlySpan<ItemStack>.Enumerator GetEnumerator() => transactions.Slots.GetEnumerator();

		IEnumerator<ItemStack> IEnumerable<ItemStack>.GetEnumerator() => transactions.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => transactions.GetEnumerator();
	}
}