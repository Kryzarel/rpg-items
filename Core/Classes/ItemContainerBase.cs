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
		public int Size => transactions.Size;

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

		public bool TryRemoveSlot(int index, int count, out ItemStack result, out ItemStack removed)
		{
			using ItemTransaction transaction = new(transactions);
			if (!transactions.TryRemoveSlot(transaction, index, count, out result, out removed))
			{
				return false;
			}
			transaction.Commit();
			return true;
		}

		public bool TryMove(int fromIndex, int count, IItemContainer to)
		{
			using ItemTransaction transaction = new(transactions);

			if (!transactions.TryRemoveSlot(transaction, fromIndex, count, out _, out ItemStack removed))
			{
				return false;
			}

			if (!to.Transactions.TryAdd(transaction, removed, out ItemStack remainder))
			{
				return false;
			}

			if (!remainder.IsEmpty && (!transactions.TryAddSlot(transaction, fromIndex, remainder, out _, out ItemStack rem) || !rem.IsEmpty))
			{
				return false;
			}

			transaction.Commit();
			return true;
		}

		public bool TryMoveSlot(int fromIndex, int count, IItemContainer to, int toIndex)
		{
			using ItemTransaction transaction = new(transactions);

			if (!transactions.TryRemoveSlot(transaction, fromIndex, count, out ItemStack result, out ItemStack fromSlot))
			{
				return false;
			}

			if (to.Transactions.TryAddSlot(transaction, toIndex, fromSlot, out _, out ItemStack remainder))
			{
				if (remainder.IsEmpty || transactions.TryAddSlot(transaction, fromIndex, remainder, out _, out remainder) && remainder.IsEmpty)
				{
					transaction.Commit();
					return true;
				}
			}
			else if (result.IsEmpty && to.Transactions.TryRemoveSlot(transaction, toIndex, -1, out _, out ItemStack toSlot))
			{
				if (!transactions.TryAddSlot(transaction, fromIndex, toSlot, out _, out remainder) || !remainder.IsEmpty)
				{
					return false;
				}

				if (!to.Transactions.TryAddSlot(transaction, toIndex, fromSlot, out _, out remainder) || !remainder.IsEmpty)
				{
					return false;
				}

				transaction.Commit();
				return true;
			}

			return false;
		}

		public void CopyFrom(ReadOnlySpan<ItemStack> stacks)
		{
			using ItemTransaction transaction = new(transactions);
			int count = stacks.Length;

			if (transactions.Size < count)
			{
				transactions.TrySetSize(transaction, count);
			}

			for (int i = 0; i < count; i++)
			{
				ItemStack stack = stacks[i];
				transactions.SetSlot(transaction, i, stack);
			}

			transaction.Commit();
		}

		public void CopyFrom(IReadOnlyList<ItemStack> stacks)
		{
			using ItemTransaction transaction = new(transactions);
			int count = stacks.Count;

			if (transactions.Size < count)
			{
				transactions.TrySetSize(transaction, count);
			}

			for (int i = 0; i < count; i++)
			{
				ItemStack stack = stacks[i];
				transactions.SetSlot(transaction, i, stack);
			}

			transaction.Commit();
		}

		public void Clear()
		{
			using ItemTransaction transaction = new(transactions);
			for (int i = 0; i < transactions.Size; i++)
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