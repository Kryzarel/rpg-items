using Kryz.RPG.Items.Interfaces;
using Kryz.RPG.Items.Structs;

namespace Kryz.RPG.Items.Views
{
	public interface IItemSlotView
	{
		int Index { get; }
		IInventory? Inventory { get; }
		ItemStack ItemStack { get; }
	}
}