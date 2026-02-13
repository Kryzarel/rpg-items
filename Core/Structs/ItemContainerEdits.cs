using Kryz.Utils;

namespace Kryz.RPG.Items
{
	public struct ItemContainerEdits
	{
		private readonly PooledList<int> editIndexes;
		private readonly PooledList<SlotEdit> slotEdits;

		private SizeEdit? sizeEdit;

		public readonly IItemContainerTransactional Container;

		public ItemContainerEdits(IItemContainerTransactional container)
		{
			Container = container;
			editIndexes = PooledList<int>.Rent(container.Size);
			editIndexes.SetCount(container.Size);
			slotEdits = PooledList<SlotEdit>.Rent();
			sizeEdit = default;
		}

		public void RecordSize(int size)
		{
			int oldSize = sizeEdit.HasValue ? sizeEdit.Value.OldSize : Container.Size;
			sizeEdit = new SizeEdit(oldSize, size);
			editIndexes.SetCount(size);
		}

		public readonly void RecordSlot(int index, ItemStack oldStack, ItemStack newStack)
		{
			ref int editIndex = ref editIndexes[index];

			if (editIndex > 0)
			{
				ref SlotEdit edit = ref slotEdits[editIndex - 1];
				edit = new SlotEdit(index, edit.OldStack, newStack);
			}
			else
			{
				slotEdits.Add(new SlotEdit(index, oldStack, newStack));
				editIndex = slotEdits.Count;
			}
		}

		public readonly void Notify() => Container.Notify(slotEdits.AsSpan(), sizeEdit);
		public readonly void Rollback() => Container.Rollback(slotEdits.AsSpan(), sizeEdit);

		public readonly void Dispose()
		{
			editIndexes.Dispose();
			slotEdits.Dispose();
		}
	}
}