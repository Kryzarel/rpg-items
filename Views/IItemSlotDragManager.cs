namespace Kryz.RPG.Items.Views
{
	public interface IItemSlotDragManager
	{
		IItemSlotView? CursorSlot { get; }
		IItemSlotView? SourceSlot { get; }

		bool StartDrag(IItemSlotView slotView, int count);
		bool StopDrag();
	}
}