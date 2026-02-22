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
			commit = false;
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

		private static readonly ItemContainerEdit defaultEdit = default;

		private readonly void Notify()
		{
			Span<ItemContainerEdit> edits = this.edits.AsSpan();
			TimSortLite.Sort(edits); // This sort must be stable, otherwise the deduplication will be incorrect
			int? firstSizeInBlock = null;
			ItemStack? firstSlotInBlock = null;

			for (int i = 0, len = edits.Length; i < len; i++)
			{
				ref readonly ItemContainerEdit edit1 = ref edits[i];
				ref readonly ItemContainerEdit edit2 = ref i + 1 < len ? ref edits[i + 1] : ref defaultEdit;

				bool isLastInContainer = edit1.Container != edit2.Container;

				if (edit1.IsSizeEdit)
				{
					firstSizeInBlock ??= edit1.SizeEdit.OldSize;
					if (isLastInContainer || !edit2.IsSizeEdit)
					{
						edit1.Container.NotifySize(firstSizeInBlock.Value, edit1.SizeEdit.NewSize);
						firstSizeInBlock = null;
					}
				}
				else
				{
					firstSlotInBlock ??= edit1.SlotEdit.OldStack;
					if (isLastInContainer || edit2.IsSizeEdit || edit1.SlotEdit.Index != edit2.SlotEdit.Index)
					{
						edit1.Container.NotifySlot(edit1.SlotEdit.Index, firstSlotInBlock.Value, edit1.SlotEdit.NewStack);
						firstSlotInBlock = null;
					}
				}

				if (isLastInContainer)
				{
					edit1.Container.NotifyTransactionSuccess();
					firstSizeInBlock = null;
					firstSlotInBlock = null;
				}
			}
		}

		private readonly void Rollback()
		{
			Span<ItemContainerEdit> edits = this.edits.AsSpan();
			TimSortLite.Sort(edits); // This sort must be stable, otherwise the deduplication will be incorrect
			int? firstSizeInBlock = null;
			ItemStack? firstSlotInBlock = null;

			for (int i = 0, len = edits.Length; i < len; i++)
			{
				ref readonly ItemContainerEdit edit1 = ref edits[i];
				ref readonly ItemContainerEdit edit2 = ref i + 1 < len ? ref edits[i + 1] : ref defaultEdit;

				bool isLastInContainer = edit1.Container != edit2.Container;

				if (edit1.IsSizeEdit)
				{
					firstSizeInBlock ??= edit1.SizeEdit.OldSize;
					if (isLastInContainer || !edit2.IsSizeEdit)
					{
						edit1.Container.RollbackSize(firstSizeInBlock.Value);
						firstSizeInBlock = null;
					}
				}
				else
				{
					firstSlotInBlock ??= edit1.SlotEdit.OldStack;
					if (isLastInContainer || edit2.IsSizeEdit || edit1.SlotEdit.Index != edit2.SlotEdit.Index)
					{
						edit1.Container.RollbackSlot(edit1.SlotEdit.Index, firstSlotInBlock.Value);
						firstSlotInBlock = null;
					}
				}

				if (isLastInContainer)
				{
					firstSizeInBlock = null;
					firstSlotInBlock = null;
				}
			}
		}
	}
}