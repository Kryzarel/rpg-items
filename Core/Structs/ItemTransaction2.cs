using System;
using System.Diagnostics;
using Kryz.Utils;

namespace Kryz.RPG.Items
{
	public struct ItemTransaction2 : IItemTransaction, IDisposable
	{
		private readonly PooledList<ItemContainerEdit> edits;

		private bool commited;
		private bool completed;

		public ItemTransaction2(IItemContainerTransactional container)
		{
			edits = PooledList<ItemContainerEdit>.Rent();
			commited = false;
			completed = false;
		}

		public readonly void RecordSize(IItemContainerTransactional container, int oldSize, int newSize)
		{
			edits.Add(new ItemContainerEdit(container, new SizeEdit(oldSize, newSize)));
		}

		public readonly void RecordSlot(IItemContainerTransactional container, int index, ItemStack oldStack, ItemStack newStack)
		{
			edits.Add(new ItemContainerEdit(container, new SlotEdit(index, oldStack, newStack)));
		}

		public void Commit()
		{
			if (completed)
			{
				Debug.Fail("Transaction already completed.");
				return;
			}

			commited = true;
			completed = true;
			Notify();
		}

		public void Dispose()
		{
			completed = true;

			if (!commited)
			{
				Rollback();
			}

			edits.Dispose();
		}

		private readonly void Notify()
		{
			ReadOnlySpan<ItemContainerEdit> edits = this.edits.AsSpan();

			for (int i = 0; i < edits.Length; i++)
			{
				ref readonly ItemContainerEdit edit = ref edits[i];

				if (edit.IsSizeEdit)
				{
					edit.Container.NotifySize(edit.SizeEdit.OldSize, edit.SizeEdit.NewSize);
				}
				else
				{
					edit.Container.NotifySlot(edit.SlotEdit.Index, edit.SlotEdit.OldStack, edit.SlotEdit.NewStack);
				}
			}
		}

		private readonly void Rollback()
		{
			ReadOnlySpan<ItemContainerEdit> edits = this.edits.AsSpan();

			for (int i = edits.Length - 1; i >= 0; i--)
			{
				ref readonly ItemContainerEdit edit = ref edits[i];

				if (edit.IsSizeEdit)
				{
					edit.Container.RollbackSize(edit.SizeEdit.OldSize);
				}
				else
				{
					edit.Container.RollbackSlot(edit.SlotEdit.Index, edit.SlotEdit.OldStack);
				}
			}
		}
	}
}