using System;
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

		public readonly void Notify() => Container.Notify(slotEdits.AsSpan(0, slotEditsCount), sizeEdit);
		public readonly void Rollback() => Container.Rollback(slotEdits.AsSpan(0, slotEditsCount), sizeEdit);

		public readonly void Dispose()
		{
			intPool.Return(editIndexes, clearArray: true);
			slotEditPool.Return(slotEdits, clearArray: true);
		}
	}
}