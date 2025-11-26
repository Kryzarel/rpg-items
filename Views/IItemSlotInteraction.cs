using UnityEngine.EventSystems;

namespace Kryz.RPG.Items.Views
{
	public interface IItemSlotInteraction
	{
		void OnPointerClick(IItemSlotView itemSlotView, PointerEventData eventData);
		void OnPointerEnter(IItemSlotView itemSlotView, PointerEventData eventData);
		void OnPointerExit(IItemSlotView itemSlotView, PointerEventData eventData);
		void OnBeginDrag(IItemSlotView itemSlotView, PointerEventData eventData);
		void OnEndDrag(IItemSlotView itemSlotView, PointerEventData eventData);
		void OnDrag(IItemSlotView itemSlotView, PointerEventData eventData);
		void OnDrop(IItemSlotView itemSlotView, PointerEventData eventData);
	}
}