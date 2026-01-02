namespace Kryz.RPG.Items.Views
{
	public interface IItemSlotView
	{
		int Index { get; }
		IInventory? Inventory { get; }
		IItemSlotInput? Input { get; }
		ItemStack ItemStack { get; }
	}
}