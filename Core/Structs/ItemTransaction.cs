using System;
using System.Diagnostics;
using Kryz.Utils;

namespace Kryz.RPG.Items
{
	public ref struct ItemTransaction
	{
		private readonly PooledList<ItemContainerTransaction> transactions;

		private bool completed;
		private bool commited;

		public ItemTransaction(IItemContainerMutable container)
		{
			transactions = PooledList<ItemContainerTransaction>.Rent();
			transactions.Add(new ItemContainerTransaction(container));
			completed = false;
			commited = false;
		}

		public readonly void RecordSize(IItemContainerMutable container, int size)
		{
			GetOrAddTransaction(container).RecordSize(size);
		}

		public readonly void RecordSlot(IItemContainerMutable container, int index, ItemStack newStack)
		{
			GetOrAddTransaction(container).RecordSlot(index, newStack);
		}

		private readonly ref ItemContainerTransaction GetOrAddTransaction(IItemContainerMutable container)
		{
			Span<ItemContainerTransaction> span = transactions.AsSpan();

			for (int i = 0; i < span.Length; i++)
			{
				ref ItemContainerTransaction transaction = ref span[i];
				if (transaction.Container == container)
				{
					return ref transaction;
				}
			}

			transactions.Add(new ItemContainerTransaction(container));
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
			commited = true;

			ReadOnlySpan<ItemContainerTransaction> t = transactions.AsSpan();

			for (int i = 0; i < t.Length; i++)
			{
				ref readonly ItemContainerTransaction transaction = ref t[i];
				Notify(transaction);
			}
		}

		public void Dispose()
		{
			completed = true;

			ReadOnlySpan<ItemContainerTransaction> t = transactions.AsSpan();

			for (int i = 0; i < t.Length; i++)
			{
				ref readonly ItemContainerTransaction transaction = ref t[i];
				if (!commited)
				{
					Rollback(transaction);
				}
				transaction.Dispose();
			}

			transactions.Dispose();
		}

		private static void Notify(in ItemContainerTransaction transaction)
		{
			if (transaction.SizeEdit.HasValue)
			{
				SizeEdit edit = transaction.SizeEdit.Value;
				transaction.Container.NotifySizeChanged(edit.OldSize, edit.NewSize);
			}

			ReadOnlySpan<SlotEdit> edits = transaction.SlotEdits;

			for (int i = 0; i < edits.Length; i++)
			{
				ref readonly SlotEdit edit = ref edits[i];
				transaction.Container.NotifySlotChanged(edit.Index, edit.OldStack, edit.NewStack);
			}
		}

		private static void Rollback(in ItemContainerTransaction transaction)
		{
			if (transaction.SizeEdit.HasValue)
			{
				transaction.Container.Size = transaction.SizeEdit.Value.OldSize;
			}

			ReadOnlySpan<SlotEdit> edits = transaction.SlotEdits;
			Span<ItemStack> slots = transaction.Container.Slots;

			for (int i = 0; i < edits.Length; i++)
			{
				ref readonly SlotEdit edit = ref edits[i];

				if ((uint)edit.Index < (uint)slots.Length)
				{
					slots[edit.Index] = edit.OldStack;
				}
			}
		}
	}
}