namespace Kryz.RPG.Items.UGUI
{
	public interface IItemSlotDragController
	{
		IItemSlotView? CursorSlot { get; }
		IItemSlotView? SourceSlot { get; }

		bool StartDrag(IItemSlotView slotView);
		bool StopDrag();
	}
}