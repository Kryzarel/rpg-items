using UnityEngine;
using UnityEngine.EventSystems;

namespace Kryz.RPG.Items.Views
{
	public class ItemSlotDragManager : MonoBehaviour, IItemSlotDragManager
	{
		[SerializeField]
		private BaseItemSlotView? dragItemSlot;

		private void Awake()
		{
			if (dragItemSlot != null)
			{
				dragItemSlot.gameObject.SetActive(false);
			}
		}

		public void StartDrag(IItemSlotView view, PointerEventData eventData)
		{
			if (dragItemSlot != null && view is BaseItemSlotView baseView)
			{
				Vector3 sourceScale = baseView.RectTransform.lossyScale;
				Vector3 targetScale = dragItemSlot.RectTransform.parent.lossyScale;

				dragItemSlot.RectTransform.sizeDelta = baseView.RectTransform.sizeDelta;
				dragItemSlot.RectTransform.localScale = new Vector3(
					sourceScale.x / targetScale.x,
					sourceScale.y / targetScale.y,
					sourceScale.z / targetScale.z);

				dragItemSlot.SetItemStack(view.ItemStack);

				dragItemSlot.gameObject.SetActive(true);
				dragItemSlot.enabled = true;
			}
		}

		public void StopDrag(IItemSlotView view, PointerEventData eventData)
		{
			if (dragItemSlot != null)
			{
				dragItemSlot.gameObject.SetActive(false);
			}
		}

		public void OnDrag(IItemSlotView view, PointerEventData eventData)
		{
			if (dragItemSlot != null)
			{
				dragItemSlot.transform.position = eventData.position;
			}
		}
	}
}