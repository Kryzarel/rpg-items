using System;
using System.Diagnostics;
using Kryz.Utils;

namespace Kryz.RPG.Items
{
	public ref struct ItemContainerTransaction
	{
		private readonly PooledList<int> editIndexes;
		private readonly PooledList<SlotEdit> slotEdits;
		private SizeEdit? sizeEdit;
		private bool completed;

		public readonly IItemContainer Container;

		public readonly ReadOnlySpan<SlotEdit> SlotEdits => slotEdits.AsSpan();
		public readonly SizeEdit? SizeEdit => sizeEdit;

		public ItemContainerTransaction(IItemContainer container)
		{
			Container = container;
			editIndexes = PooledList<int>.Rent(container.Size);
			editIndexes.SetCount(container.Size);
			slotEdits = PooledList<SlotEdit>.Rent();
			sizeEdit = default;
			completed = false;
		}

		public readonly int GetSize()
		{
			return sizeEdit.HasValue ? sizeEdit.Value.NewSize : Container.Size;
		}

		public void SetSize(int size)
		{
			sizeEdit = new SizeEdit(Container.Size, size);
			editIndexes.SetCount(size);
		}

		public readonly ItemStack GetSlot(int index)
		{
			int editIndex = editIndexes[index];
			return editIndex > 0 ? slotEdits[editIndex - 1].NewStack : Container[index];
		}

		public readonly void SetSlot(int index, ItemStack value)
		{
			SlotEdit edit = new(index, Container[index], value);
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

		public void Commit()
		{
			if (completed)
			{
				Debug.Fail("Transaction already completed.");
				return;
			}

			completed = true;
			Container.Commit(SlotEdits, SizeEdit);
		}

		public void Dispose()
		{
			completed = true;
			editIndexes.Dispose();
			slotEdits.Dispose();
		}
	}
}