using System.Collections.Generic;

namespace Kryz.RPG.Items.Views
{
	public interface IInventoryView
	{
		IItemContainer? ItemContainer { get; set; }
		IReadOnlyList<IItemSlotView> ItemSlots { get; }
	}
}