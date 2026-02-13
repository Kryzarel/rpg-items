using System;
using System.Diagnostics;
using Kryz.Utils;

namespace Kryz.RPG.Items
{
	public struct ItemTransaction : IItemTransaction, IDisposable
	{
		private readonly PooledList<ItemContainerEdits> edits;

		private bool completed;
		private bool commited;

		public ItemTransaction(IItemContainerTransactional container)
		{
			edits = PooledList<ItemContainerEdits>.Rent();
			edits.Add(new ItemContainerEdits(container));
			completed = false;
			commited = false;
		}

		public readonly void RecordSize(IItemContainerTransactional container, int size)
		{
			GetOrAddTransaction(container).RecordSize(size);
		}

		public readonly void RecordSlot(IItemContainerTransactional container, int index, ItemStack newStack)
		{
			GetOrAddTransaction(container).RecordSlot(index, newStack);
		}

		private readonly ref ItemContainerEdits GetOrAddTransaction(IItemContainerTransactional container)
		{
			Span<ItemContainerEdits> span = edits.AsSpan();

			for (int i = 0; i < span.Length; i++)
			{
				ref ItemContainerEdits transaction = ref span[i];
				if (transaction.Container == container)
				{
					return ref transaction;
				}
			}

			edits.Add(new ItemContainerEdits(container));
			return ref edits[^1];
		}

		public void Commit()
		{
			if (completed)
			{
				Debug.Fail("Transaction already completed.");
				return;
			}

			completed = true;
			commited = true;

			ReadOnlySpan<ItemContainerEdits> editSpan = edits.AsSpan();
			for (int i = 0; i < editSpan.Length; i++)
			{
				ref readonly ItemContainerEdits edit = ref editSpan[i];
				edit.Notify();
			}
		}

		public void Dispose()
		{
			completed = true;

			ReadOnlySpan<ItemContainerEdits> editSpan = edits.AsSpan();
			for (int i = 0; i < editSpan.Length; i++)
			{
				ref readonly ItemContainerEdits edit = ref editSpan[i];
				if (!commited)
				{
					edit.Rollback();
				}
				edit.Dispose();
			}

			edits.Dispose();
		}
	}
}