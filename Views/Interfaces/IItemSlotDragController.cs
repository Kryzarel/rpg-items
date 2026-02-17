namespace Kryz.RPG.Items.Views
{
	public interface IItemSlotDragController
	{
		IItemSlotView? CursorSlot { get; }
		IItemSlotView? SourceSlot { get; }

		bool StartDrag(IItemSlotView slotView);
		bool StopDrag();
	}
}