using System;
using System.Buffers;
using Kryz.Utils;

namespace Kryz.RPG.Items
{
	public struct ItemContainerEdit : IDisposable
	{
		private SizeEdit? sizeEdit;
		private SlotEdit[] slotEdits;
		private int[] edited;
		private int editedCount;

		public readonly IItemContainerTransactional Container;

		public readonly SizeEdit? SizeEdit => sizeEdit;
		public readonly ReadOnlySpan<SlotEdit> SlotEdits => new(slotEdits, 0, Container.Size);
		public readonly ReadOnlySpan<int> EditedIndexes => new(edited, 0, editedCount);

		public ItemContainerEdit(IItemContainerTransactional container)
		{
			sizeEdit = default;
			int size = container.Size;
			slotEdits = ArrayPool<SlotEdit>.Shared.Rent(size);
			edited = ArrayPool<int>.Shared.Rent(size);
			editedCount = 0;
			Container = container;
		}

		public void RecordSlot(int index, ItemStack oldStack, ItemStack newStack)
		{
			ref SlotEdit edit = ref slotEdits[index];

			if (edit.Index == default && edit.OldStack == default && edit.NewStack == default)
			{
				edit = new SlotEdit(index, oldStack, newStack);
				edited[editedCount++] = index;
			}
			else
			{
				edit = new SlotEdit(index, edit.OldStack, newStack);
			}
		}

		public void RecordSize(int oldSize, int newSize)
		{
			sizeEdit = sizeEdit.HasValue ? new SizeEdit(sizeEdit.Value.OldSize, newSize) : new SizeEdit(oldSize, newSize);

			if (newSize > slotEdits.Length)
			{
				ArrayPoolUtils.Resize(ref slotEdits, oldSize, newSize);
				ArrayPoolUtils.Resize(ref edited, oldSize, newSize);
			}
		}

		public void Dispose()
		{
			sizeEdit = default;

			for (int i = 0; i < editedCount; i++)
			{
				slotEdits[edited[i]] = default;
			}

			ArrayPool<SlotEdit>.Shared.Return(slotEdits);
			ArrayPool<int>.Shared.Return(edited);
			editedCount = 0;
		}
	}
}