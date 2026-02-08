using System;

namespace Kryz.RPG.Items
{
	public interface IItemContainerMutable
	{
		ref ItemStack this[int index] { get; }

		Span<ItemStack> Slots { get; }

		int Size { get; set; }

		void NotifySlotChanged(int index, ItemStack oldStack, ItemStack newStack);
		void NotifySizeChanged(int oldSize, int newSize);
	}
}