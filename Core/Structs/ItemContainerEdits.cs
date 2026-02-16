using System.Buffers;
using Kryz.Utils;

namespace Kryz.RPG.Items
{
	public struct ItemContainerEdits
	{
		private static readonly ArrayPool<int> intPool = ArrayPool<int>.Shared;
		private static readonly ArrayPool<SlotEdit> slotEditPool = ArrayPool<SlotEdit>.Shared;

		private int[] editIndexes;
		private SlotEdit[] slotEdits;
		private int slotEditsCount;
		private SizeEdit? sizeEdit;

		public readonly IItemContainerTransactional Container;

		public ItemContainerEdits(IItemContainerTransactional container)
		{
			editIndexes = intPool.Rent(container.Size);
			slotEdits = slotEditPool.Rent(container.Size);
			slotEditsCount = 0;
			sizeEdit = default;

			Container = container;
		}

		public void RecordSize(int oldSize, int newSize)
		{
			oldSize = sizeEdit.HasValue ? sizeEdit.Value.OldSize : oldSize;
			sizeEdit = new SizeEdit(oldSize, newSize);
		}

		public void RecordSlot(int index, ItemStack oldStack, ItemStack newStack)
		{
			if (index >= editIndexes.Length)
			{
				ArrayPoolUtils.Resize(ref editIndexes, editIndexes.Length, index + 1);
			}

			ref int editIndex = ref editIndexes[index];

			if (editIndex > 0)
			{
				ref SlotEdit edit = ref slotEdits[editIndex - 1];
				edit = new SlotEdit(index, edit.OldStack, newStack);
			}
			else
			{
				if (slotEditsCount >= slotEdits.Length)
				{
					ArrayPoolUtils.Resize(ref slotEdits, slotEdits.Length, slotEditsCount + 1);
				}

				slotEdits[slotEditsCount++] = new SlotEdit(index, oldStack, newStack);
				editIndex = slotEditsCount;
			}
		}

		public readonly void Notify()
		{
			if (sizeEdit.HasValue)
			{
				SizeEdit edit = sizeEdit.Value;
				Container.NotifySize(edit.OldSize, edit.NewSize);
			}

			for (int i = 0; i < slotEditsCount; i++)
			{
				ref readonly SlotEdit edit = ref slotEdits[i];
				Container.NotifySlot(edit.Index, edit.OldStack, edit.NewStack);
			}
		}

		public readonly void Rollback()
		{
			if (sizeEdit.HasValue)
			{
				Container.RollbackSize(sizeEdit.Value.OldSize);
			}

			for (int i = 0; i < slotEditsCount; i++)
			{
				ref readonly SlotEdit edit = ref slotEdits[i];
				Container.RollbackSlot(edit.Index, edit.OldStack);
			}
		}

		public readonly void Dispose()
		{
			intPool.Return(editIndexes, clearArray: true);
			slotEditPool.Return(slotEdits, clearArray: true);
		}
	}
}