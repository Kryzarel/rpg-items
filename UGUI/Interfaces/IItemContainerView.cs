using System.Collections.Generic;

namespace Kryz.RPG.Items.UGUI
{
	public interface IItemContainerView
	{
		IItemContainer? ItemContainer { get; set; }
		IReadOnlyList<IItemSlotView> ItemSlots { get; }
	}
}