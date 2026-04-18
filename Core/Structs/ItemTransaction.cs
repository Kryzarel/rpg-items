using System;
using System.Diagnostics;
using Kryz.Utils;

namespace Kryz.RPG.Items
{
	public struct ItemTransaction : IItemTransaction, IDisposable
	{
		private readonly PooledList<ItemContainerEdit> edits;

		private bool commit;
		private bool completed;

		public ItemTransaction(IItemContainerTransactional container)
		{
			edits = PooledList<ItemContainerEdit>.Rent();
			edits.Add(new ItemContainerEdit(container));
			commit = false;
			completed = false;
		}

		public readonly void RecordSize(IItemContainerTransactional container, int oldSize, int newSize)
		{
			ref ItemContainerEdit edit = ref GetOrCreate(container);
			edit.RecordSize(oldSize, newSize);
		}

		public readonly void RecordSlot(IItemContainerTransactional container, int index, ItemStack oldStack, ItemStack newStack)
		{
			ref ItemContainerEdit edit = ref GetOrCreate(container);
			edit.RecordSlot(index, oldStack, newStack);
		}

		public bool SetResult(bool value)
		{
			commit = value;
			return value;
		}

		public void Dispose()
		{
			if (completed)
			{
				Debug.Fail("Transaction already completed.");
				return;
			}

			completed = true;

			if (commit)
			{
				Notify();
			}
			else
			{
				Rollback();
			}

			edits.Dispose();
		}

		private readonly ref ItemContainerEdit GetOrCreate(IItemContainerTransactional container)
		{
			for (int i = 0, count = edits.Count; i < count; i++)
			{
				ref ItemContainerEdit edit = ref edits[i];

				if (edit.Container == container)
				{
					return ref edit;
				}
			}

			edits.Add(new ItemContainerEdit(container));
			return ref edits[^1];
		}

		private readonly void Notify()
		{
			foreach (ItemContainerEdit edit in edits.AsSpan())
			{
				if (edit.SizeEdit.HasValue)
				{
					SizeEdit sizeEdit = edit.SizeEdit.Value;
					edit.Container.NotifySize(sizeEdit.OldSize, sizeEdit.NewSize);
				}

				int size = edit.Container.Size;
				ReadOnlySpan<SlotEdit> slotEdits = edit.SlotEdits;

				foreach (int index in edit.EditedIndexes)
				{
					if (index < size)
					{
						SlotEdit slotEdit = slotEdits[index];
						if (slotEdit.OldStack != slotEdit.NewStack)
						{
							edit.Container.NotifySlot(slotEdit.Index, slotEdit.OldStack, slotEdit.NewStack);
						}
					}
				}

				edit.Container.NotifyTransactionSuccess();
				edit.Dispose();
			}
		}

		private readonly void Rollback()
		{
			foreach (ItemContainerEdit edit in edits.AsSpan())
			{
				if (edit.SizeEdit.HasValue)
				{
					SizeEdit sizeEdit = edit.SizeEdit.Value;
					edit.Container.RollbackSize(sizeEdit.OldSize);
				}

				int size = edit.Container.Size;
				ReadOnlySpan<SlotEdit> slotEdits = edit.SlotEdits;

				foreach (int index in edit.EditedIndexes)
				{
					if (index < size)
					{
						SlotEdit slotEdit = slotEdits[index];
						edit.Container.RollbackSlot(slotEdit.Index, slotEdit.OldStack);
					}
				}

				edit.Dispose();
			}
		}
	}
}