using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Kryz.RPG.Items.Views
{
	public class ItemSlotDragManager : MonoBehaviour, IItemSlotDragManager
	{
		[SerializeField]
		private Canvas? canvas;
		[SerializeField]
		private BaseItemSlotView? dragSlot;

		public bool IsDragging => dragSlot != null && dragSlot.ItemStack != default;
		public BaseItemSlotView? DragSlot => dragSlot;

		private IInventory.OnSlotChangedDelegate? onSlotChangedAction;

		private void OnValidate()
		{
			if (canvas == null) canvas = GetComponentInParent<Canvas>();
			if (dragSlot == null) dragSlot = GetComponentInChildren<BaseItemSlotView>();
		}

		private void Awake()
		{
			onSlotChangedAction = OnSlotChanged;

			if (dragSlot != null)
			{
				dragSlot.gameObject.SetActive(false);
				dragSlot.Unbind();
			}
		}

		public void StartDrag(IItemSlotView view, PointerEventData eventData)
		{
			if (dragSlot != null && view.Inventory != null && view is BaseItemSlotView baseView)
			{
				Vector3 sourceScale = baseView.RectTransform.lossyScale;
				Vector3 targetScale = dragSlot.RectTransform.parent.lossyScale;

				dragSlot.RectTransform.sizeDelta = baseView.RectTransform.sizeDelta;
				dragSlot.RectTransform.localScale = new Vector3(
					sourceScale.x / targetScale.x,
					sourceScale.y / targetScale.y,
					sourceScale.z / targetScale.z);

				view.Inventory.OnSlotChanged += onSlotChangedAction;
				dragSlot.Bind(view.SlotIndex, view.Inventory);
				dragSlot.gameObject.SetActive(true);
			}
		}

		public void StopDrag(IItemSlotView view, PointerEventData eventData)
		{
			if (view.Inventory != null)
			{
				view.Inventory.OnSlotChanged -= onSlotChangedAction;
			}

			if (dragSlot != null)
			{
				dragSlot.gameObject.SetActive(false);
				dragSlot.Unbind();
			}
		}

		public void OnDrag(IItemSlotView view, PointerEventData eventData)
		{
			// if (dragItemSlot != null)
			// {
			// 	SetScreenPosition(dragItemSlot.RectTransform, eventData.position);
			// }
		}

		private void LateUpdate()
		{
			if (dragSlot != null && dragSlot.gameObject.activeSelf)
			{
				SetScreenPosition(dragSlot.RectTransform, Mouse.current.position.value);
			}
		}

		private void SetScreenPosition(RectTransform rect, Vector2 screenPos)
		{
			Camera? cam = canvas != null ? canvas.worldCamera : null;
			RectTransform parent = (RectTransform)rect.parent;

			if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPos, cam, out Vector2 position))
			{
				rect.localPosition = position;
			}
		}

		private void OnSlotChanged(int index, ItemStack previousStack, ItemStack newStack)
		{
			if (dragSlot != null && dragSlot.Inventory != null && dragSlot.SlotIndex == index)
			{
				dragSlot.SetItemStack(newStack);
			}
		}
	}
}