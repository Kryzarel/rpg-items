using System.Collections.Generic;

namespace Kryz.RPG.Items.Views
{
	public interface IInventoryView
	{
		IInventory? Inventory { get; set; }
		IReadOnlyList<IItemSlotView> ItemSlots { get; }
	}
}