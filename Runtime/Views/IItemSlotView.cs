
namespace Kryz.RPG.Items.Views
{
	public interface IItemSlotView
	{
		int Index { get; }
		IInventory? Inventory { get; }
		ItemStack ItemStack { get; }
	}
}