namespace Kryz.RPG.Items.Views
{
	public interface IItemSlotDragManager
	{
		IItemSlotView? DragSlot { get; }
		IItemSlotView? SourceSlot { get; }

		void StartDrag(IItemSlotView slotView, ItemStack itemStack);
		void StopDrag();
	}
}