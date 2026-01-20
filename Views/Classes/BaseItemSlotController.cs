using UnityEngine;
using UnityEngine.EventSystems;

namespace Kryz.RPG.Items.Views
{
	public abstract class BaseItemSlotController : MonoBehaviour, IItemSlotController
	{
		public abstract bool OnBeginDrag(IItemSlotView itemSlotView, PointerEventData eventData);
		public abstract bool OnDeselect(IItemSlotView itemSlotView, BaseEventData eventData);
		public abstract bool OnDrag(IItemSlotView itemSlotView, PointerEventData eventData);
		public abstract bool OnDrop(IItemSlotView itemSlotView, PointerEventData eventData);
		public abstract bool OnEndDrag(IItemSlotView itemSlotView, PointerEventData eventData);
		public abstract bool OnPointerClick(IItemSlotView itemSlotView, PointerEventData eventData);
		public abstract bool OnPointerEnter(IItemSlotView itemSlotView, PointerEventData eventData);
		public abstract bool OnPointerExit(IItemSlotView itemSlotView, PointerEventData eventData);
		public abstract bool OnSelect(IItemSlotView itemSlotView, BaseEventData eventData);
	}
}