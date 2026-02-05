using System;
using System.Collections.Generic;

namespace Kryz.RPG.Items
{
	public interface IItemContainer : IReadOnlyList<ItemStack>
	{
		public delegate void OnSlotChangedDelegate(int index, ItemStack oldStack, ItemStack newStack);
		public delegate void OnSizeChangedDelegate(int oldSize, int newSize);

		event OnSlotChangedDelegate OnSlotChanged;
		event OnSizeChangedDelegate OnSizeChanged;

		public IItemContainerStrategy Strategy { get; }

		public ReadOnlySpan<ItemStack> Slots { get; }

		int Size { get; set; }
		int FreeSlots { get; }
		int OccupiedSlots { get; }

		new int Count => Size;

		void ApplySlot(int index, ItemStack stack);
		void NotifySlotChanged(int index, ItemStack oldStack, ItemStack newStack);
		void NotifySizeChanged(int oldSize, int newSize);
	}
}