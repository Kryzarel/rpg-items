using UnityEngine.EventSystems;

namespace Kryz.RPG.Items.Views
{
	public interface IItemSlotDragManager
	{
		bool IsDragging { get; }
		BaseItemSlotView? DragSlot { get; }

		void StartDrag(IItemSlotView view, PointerEventData eventData);
		void StopDrag(IItemSlotView view, PointerEventData eventData);
		void OnDrag(IItemSlotView view, PointerEventData eventData);
	}
}