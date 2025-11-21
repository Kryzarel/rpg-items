using UnityEngine.EventSystems;

namespace Kryz.RPG.Items.UI
{
	public interface IItemSlotDragManager
	{
		void StartDrag(IItemSlotView view, PointerEventData eventData);
		void StopDrag(IItemSlotView view, PointerEventData eventData);
		void OnDrag(IItemSlotView view, PointerEventData eventData);
	}
}