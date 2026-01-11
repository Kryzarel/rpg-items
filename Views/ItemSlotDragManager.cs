using UnityEngine;
using UnityEngine.InputSystem;

namespace Kryz.RPG.Items.Views
{
	public class ItemSlotDragManager : MonoBehaviour, IItemSlotDragManager
	{
		[SerializeField]
		private Canvas? canvas;
		[SerializeField]
		private BaseItemSlotView? dragSlot;

		private BaseItemSlotView? sourceSlot;

		public IItemSlotView? DragSlot => dragSlot;
		public IItemSlotView? SourceSlot => sourceSlot;

		private void OnValidate()
		{
			if (canvas == null) canvas = GetComponentInParent<Canvas>(includeInactive: true);
			if (dragSlot == null) dragSlot = GetComponentInChildren<BaseItemSlotView>(includeInactive: true);
		}

		private void Start()
		{
			if (dragSlot != null)
			{
				dragSlot.gameObject.SetActive(false);
			}
		}

		public void StartDrag(IItemSlotView slotView, ItemStack itemStack)
		{
			if (dragSlot == null || dragSlot.Inventory == null || !dragSlot.Inventory.TryAdd(itemStack, out _) || slotView is not BaseItemSlotView baseView)
			{
				return;
			}

			Vector3 sourceScale = baseView.RectTransform.lossyScale;
			Vector3 targetScale = dragSlot.RectTransform.parent.lossyScale;

			sourceSlot = baseView;

			dragSlot.RectTransform.sizeDelta = baseView.RectTransform.sizeDelta;
			dragSlot.RectTransform.localScale = new Vector3(
				sourceScale.x / targetScale.x,
				sourceScale.y / targetScale.y,
				sourceScale.z / targetScale.z);

			dragSlot.gameObject.SetActive(true);
		}

		public void StopDrag()
		{
			if (dragSlot != null)
			{
				dragSlot.gameObject.SetActive(false);
			}

			sourceSlot = null;
		}

		private void LateUpdate()
		{
			if (dragSlot != null)
			{
				bool enabled = dragSlot.ItemStack != default;

				if (enabled)
				{
					SetScreenPosition(dragSlot.RectTransform, Mouse.current.position.value);
				}

				if (dragSlot.gameObject.activeSelf != enabled)
				{
					dragSlot.gameObject.SetActive(enabled);
				}
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
	}
}