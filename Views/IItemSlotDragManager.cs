namespace Kryz.RPG.Items.Views
{
	public interface IItemSlotDragManager
	{
		IItemSlotView? DraggedSlot { get; }
		IItemSlotView? DraggedSlotCopy { get; }

		void StartDrag(IItemSlotView slotView, ItemStack itemStack = default);
		void StopDrag();
	}
}