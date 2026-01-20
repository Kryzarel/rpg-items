namespace Kryz.RPG.Items.Views
{
	public interface IItemSlotView
	{
		int SlotIndex { get; }
		IInventory? Inventory { get; }
		IItemSlotController? Controller { get; }
		ItemStack ItemStack { get; }
	}
}