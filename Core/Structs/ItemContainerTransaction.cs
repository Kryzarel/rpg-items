using System.Diagnostics;
using Kryz.Utils;

namespace Kryz.RPG.Items
{
	public ref struct ItemContainerTransaction
	{
		private readonly PooledList<IItemContainer> containers;
		private readonly PooledList<ItemContainerTransactionContext> transactions;
		private bool completed;

		private ItemContainerTransaction(PooledList<IItemContainer> containers, PooledList<ItemContainerTransactionContext> transactions)
		{
			this.containers = containers;
			this.transactions = transactions;
			completed = false;
		}

		public static ItemContainerTransaction Begin()
		{
			return new(PooledList<IItemContainer>.Rent(), PooledList<ItemContainerTransactionContext>.Rent());
		}

		public readonly ref ItemContainerTransactionContext GetContext(IItemContainer container)
		{
			for (int i = 0; i < containers.Count; i++)
			{
				if (containers[i] == container)
				{
					return ref transactions[i];
				}
			}

			containers.Add(container);
			transactions.Add(new ItemContainerTransactionContext(container));
			return ref transactions[^1];
		}

		public void Commit()
		{
			if (completed)
			{
				Debug.Fail("Transaction already completed.");
				return;
			}

			completed = true;

			for (int i = 0; i < containers.Count; i++)
			{
				ref readonly ItemContainerTransactionContext context = ref transactions[i];
				containers[i].Commit(context.SlotEdits, context.SizeEdit);
			}
		}

		public void Dispose()
		{
			completed = true;

			for (int i = 0; i < transactions.Count; i++)
			{
				transactions[i].Dispose();
			}
			containers.Dispose();
			transactions.Dispose();
		}
	}
}