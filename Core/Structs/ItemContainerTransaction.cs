using System;
using System.Diagnostics;
using Kryz.Utils;

namespace Kryz.RPG.Items
{
	public ref struct ItemContainerTransaction
	{
		private bool completed;
		private ValuePooledList<SlotEdit> edits;

		public readonly ReadOnlySpan<SlotEdit> Edits => edits.AsSpan();

		public ItemContainerTransaction(Span<SlotEdit> buffer)
		{
			completed = false;
			edits = new ValuePooledList<SlotEdit>(buffer);
		}

		public readonly ItemStack GetSlot(IItemContainer container, int index)
		{
			for (int i = edits.Count - 1; i >= 0; i--)
			{
				ref readonly SlotEdit e = ref edits[i];

				if (e.Container == container && e.Index == index)
				{
					return e.NewStack;
				}
			}

			return container[index];
		}

		public void SetSlot(IItemContainer container, int index, ItemStack value)
		{
			edits.Add(new SlotEdit(container, index, container[index], value));
		}

		public void Commit()
		{
			if (completed)
			{
				Debug.Fail("Transaction already completed.");
				return;
			}

			completed = true;
			Commit(edits.AsSpan());
		}

		public void Dispose()
		{
			completed = true;
			edits.Dispose();
		}

		private static void Commit(ReadOnlySpan<SlotEdit> edits)
		{
			// First pass: apply all slot changes
			for (int i = 0; i < edits.Length; i++)
			{
				ref readonly SlotEdit edit = ref edits[i];
				edit.Container.ApplySlot(edit.Index, edit.NewStack);
			}

			using ValuePooledList<(IItemContainer, int)> notified = new(edits.Length);

			// Second pass: fire events (in reverse order to ensure only the last write is notified)
			for (int i = edits.Length - 1; i >= 0; i--)
			{
				ref readonly SlotEdit edit = ref edits[i];

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