using System;
using System.Diagnostics;
using Kryz.Utils;

namespace Kryz.RPG.Items
{
	public ref struct ItemContainerTransaction
	{
		private bool completed;
		private ValuePooledList<SizeEdit> sizeEdits;
		private ValuePooledList<SlotEdit> slotEdits;

		public readonly ReadOnlySpan<SizeEdit> SizeEdits => sizeEdits;
		public readonly ReadOnlySpan<SlotEdit> SlotEdits => slotEdits;

		public ItemContainerTransaction(Span<SizeEdit> sizeBuffer, Span<SlotEdit> slotBuffer)
		{
			completed = false;
			sizeEdits = new ValuePooledList<SizeEdit>(sizeBuffer);
			slotEdits = new ValuePooledList<SlotEdit>(slotBuffer);
		}

		public readonly int GetSize(IItemContainer container)
		{
			ReadOnlySpan<SizeEdit> sizeEdits = this.sizeEdits;

			for (int i = 0; i < sizeEdits.Length; i++)
			{
				ref readonly SizeEdit e = ref sizeEdits[i];

				if (e.Container == container)
				{
					return e.NewSize;
				}
			}

			return container.Size;
		}

		public void SetSize(IItemContainer container, int value)
		{
			Span<SizeEdit> sizeEdits = this.sizeEdits;

			for (int i = 0; i < sizeEdits.Length; i++)
			{
				ref SizeEdit e = ref sizeEdits[i];

				if (e.Container == container)
				{
					e = new SizeEdit(container, container.Size, value);
					return;
				}
			}

			this.sizeEdits.Add(new SizeEdit(container, container.Size, value));
		}

		public readonly ItemStack GetSlot(IItemContainer container, int index)
		{
			ReadOnlySpan<SlotEdit> slotEdits = this.slotEdits;

			for (int i = slotEdits.Length - 1; i >= 0; i--)
			{
				ref readonly SlotEdit e = ref slotEdits[i];

				if (e.Container == container && e.Index == index)
				{
					return e.NewStack;
				}
			}

			return container[index];
		}

		public void SetSlot(IItemContainer container, int index, ItemStack value)
		{
			slotEdits.Add(new SlotEdit(container, index, container[index], value));
		}

		public void Commit()
		{
			if (completed)
			{
				Debug.Fail("Transaction already completed.");
				return;
			}

			completed = true;
			Commit(sizeEdits, slotEdits);
		}

		public void Dispose()
		{
			completed = true;
			slotEdits.Dispose();
		}

		private static void Commit(ReadOnlySpan<SizeEdit> sizeEdits, ReadOnlySpan<SlotEdit> slotEdits)
		{
			// First pass: apply edits
			for (int i = 0; i < sizeEdits.Length; i++)
			{
				ref readonly SizeEdit edit = ref sizeEdits[i];
				edit.Container.Size = edit.NewSize;
			}

			for (int i = 0; i < slotEdits.Length; i++)
			{
				ref readonly SlotEdit edit = ref slotEdits[i];
				edit.Container.ApplySlot(edit.Index, edit.NewStack);
			}

			// Second pass: fire events
			for (int i = 0; i < sizeEdits.Length; i++)
			{
				ref readonly SizeEdit edit = ref sizeEdits[i];
				edit.Container.NotifySizeChanged(edit.OldSize, edit.NewSize);
			}

			using ValuePooledList<(IItemContainer, int)> notified = new(slotEdits.Length);

			// Handle slot events in reverse order to ensure only the last write is notified
			for (int i = slotEdits.Length - 1; i >= 0; i--)
			{
				ref readonly SlotEdit edit = ref slotEdits[i];

				if (!AlreadyNotified(edit, notified.AsSpan()))
				{
					notified.Add((edit.Container, edit.Index));
					edit.Container.NotifySlotChanged(edit.Index, edit.OldStack, edit.NewStack);
				}
			}
		}

		private static bool AlreadyNotified(in SlotEdit current, ReadOnlySpan<(IItemContainer, int)> notified)
		{
			for (int i = 0; i < notified.Length; i++)
			{
				ref readonly (IItemContainer, int) other = ref notified[i];

				if (other.Item1 == current.Container && other.Item2 == current.Index)
				{
					return true;
				}
			}
			return false;
		}
	}
}