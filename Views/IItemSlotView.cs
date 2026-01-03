namespace Kryz.RPG.Items.Views
{
	public interface IItemSlotView
	{
		int SlotIndex { get; }
		IInventory? Inventory { get; }
		IItemSlotInput? Input { get; }
		ItemStack ItemStack { get; }
	}
}