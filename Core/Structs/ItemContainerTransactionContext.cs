using System;
using Kryz.Utils;

namespace Kryz.RPG.Items
{
	public struct ItemContainerTransactionContext
	{
		private readonly IItemContainer container;
		private readonly PooledList<int> editIndexes;
		private readonly PooledList<SlotEdit> slotEdits;
		private SizeEdit? sizeEdit;

		public readonly ReadOnlySpan<SlotEdit> SlotEdits => slotEdits.AsSpan();
		public readonly SizeEdit? SizeEdit => sizeEdit;

		public ItemContainerTransactionContext(IItemContainer container)
		{
			this.container = container;
			editIndexes = PooledList<int>.Rent(container.Size);
			editIndexes.SetCount(container.Size);
			slotEdits = PooledList<SlotEdit>.Rent();
			sizeEdit = default;
		}

		public readonly int GetSize()
		{
			return sizeEdit.HasValue ? sizeEdit.Value.NewSize : container.Size;
		}

		public void SetSize(int size)
		{
			sizeEdit = new SizeEdit(container.Size, size);
			editIndexes.SetCount(size);
		}

		public readonly ItemStack GetSlot(int index)
		{
			int editIndex = editIndexes[index];
			return editIndex > 0 ? slotEdits[editIndex - 1].NewStack : container[index];
		}

		public readonly void SetSlot(int index, ItemStack value)
		{
			SlotEdit edit = new(index, container[index], value);
			ref int editIndex = ref editIndexes[index];

			if (editIndex > 0)
			{
				slotEdits[editIndex - 1] = edit;
			}
			else
			{
				slotEdits.Add(edit);
				editIndex = slotEdits.Count;
			}
		}

		public readonly void Dispose()
		{
			editIndexes.Dispose();
			slotEdits.Dispose();
		}
	}
}