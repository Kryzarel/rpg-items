using System;
using Kryz.Utils;

namespace Kryz.RPG.Items
{
	public struct ItemContainerTransactionContext
	{
		private readonly IItemContainer container;
		private readonly PooledList<SlotEdit> slotEdits;
		private SizeEdit? sizeEdit;

		public readonly ReadOnlySpan<SlotEdit> SlotEdits => slotEdits.AsSpan();
		public readonly SizeEdit? SizeEdit => sizeEdit;

		public ItemContainerTransactionContext(IItemContainer container)
		{
			this.container = container;
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
		}

		public readonly ItemStack GetSlot(int index)
		{
			ReadOnlySpan<SlotEdit> slotEdits = this.slotEdits.AsSpan();

			for (int i = 0; i < slotEdits.Length; i++)
			{
				ref readonly SlotEdit edit = ref slotEdits[i];
				if (edit.Index == index)
				{
					return edit.NewStack;
				}
			}

			return container[index];
		}

		public readonly void SetSlot(int index, ItemStack value)
		{
			SlotEdit slotEdit = new(index, container[index], value);
			Span<SlotEdit> slotEdits = this.slotEdits.AsSpan();

			for (int i = 0; i < slotEdits.Length; i++)
			{
				ref SlotEdit edit = ref slotEdits[i];
				if (edit.Index == index)
				{
					edit = slotEdit;
					return;
				}
			}

			this.slotEdits.Add(slotEdit);
		}

		public readonly void Dispose()
		{
			slotEdits.Dispose();
		}
	}
}