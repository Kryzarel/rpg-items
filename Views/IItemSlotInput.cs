using UnityEngine.EventSystems;

namespace Kryz.RPG.Items.Views
{
	public interface IItemSlotInput
	{
		bool OnPointerClick(IItemSlotView itemSlotView, PointerEventData eventData);
		bool OnPointerEnter(IItemSlotView itemSlotView, PointerEventData eventData);
		bool OnPointerExit(IItemSlotView itemSlotView, PointerEventData eventData);
		bool OnBeginDrag(IItemSlotView itemSlotView, PointerEventData eventData);
		bool OnEndDrag(IItemSlotView itemSlotView, PointerEventData eventData);
		bool OnDrag(IItemSlotView itemSlotView, PointerEventData eventData);
		bool OnDrop(IItemSlotView itemSlotView, PointerEventData eventData);
		bool OnSelect(IItemSlotView itemSlotView, BaseEventData eventData);
		bool OnDeselect(IItemSlotView itemSlotView, BaseEventData eventData);
	}
}