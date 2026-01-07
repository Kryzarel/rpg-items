using UnityEngine.EventSystems;

namespace Kryz.RPG.Items.Views
{
	public interface IItemSlotDragManager
	{
		IItemSlotView? DraggedSlot { get; }

		void StartDrag(IItemSlotView slotView, PointerEventData eventData);
		void StopDrag();
	}
}