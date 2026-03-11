namespace Kryz.RPG.Items.UGUI
{
	public interface IItemSlotView
	{
		int SlotIndex { get; }
		IItemContainer? ItemContainer { get; }
		IItemSlotController? Controller { get; }
		ItemStack VisualItemStack { get; }
		ItemStack RealItemStack { get; }

		void Bind(int slotIndex, IItemContainer itemContainer, IItemSlotController? controller = null);
		void Unbind();
		void SetItemStack(ItemStack stack);
	}
}