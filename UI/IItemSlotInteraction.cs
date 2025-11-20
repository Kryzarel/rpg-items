using UnityEngine.EventSystems;

namespace Kryz.RPG.Items.UI
{
	public interface IItemSlotInteraction
	{
		void OnPointerClick(ItemSlotView itemSlotView, PointerEventData eventData);
		void OnPointerEnter(ItemSlotView itemSlotView, PointerEventData eventData);
		void OnPointerExit(ItemSlotView itemSlotView, PointerEventData eventData);
		void OnBeginDrag(ItemSlotView itemSlotView, PointerEventData eventData);
		void OnEndDrag(ItemSlotView itemSlotView, PointerEventData eventData);
		void OnDrag(ItemSlotView itemSlotView, PointerEventData eventData);
		void OnDrop(ItemSlotView itemSlotView, PointerEventData eventData);
	}
}