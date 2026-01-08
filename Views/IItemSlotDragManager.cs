namespace Kryz.RPG.Items.Views
{
	public interface IItemSlotDragManager
	{
		IItemSlotView? DraggedSlot { get; }
		ItemStack DraggedItemStack { get; }

		void StartDrag(IItemSlotView slotView, ItemStack itemStack = default);
		void StopDrag();
	}
}