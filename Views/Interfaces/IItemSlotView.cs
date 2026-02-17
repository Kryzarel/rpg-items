namespace Kryz.RPG.Items.Views
{
	public interface IItemSlotView
	{
		int SlotIndex { get; }
		IItemContainer? ItemContainer { get; }
		IItemSlotController? Controller { get; }
		ItemStack ItemStack { get; }

		void Bind(int slotIndex, IItemContainer itemContainer, IItemSlotController? controller = null);
		void Unbind();
		void SetItemStack(ItemStack stack);
	}
}