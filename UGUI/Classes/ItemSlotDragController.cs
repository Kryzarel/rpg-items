using UnityEngine;
using UnityEngine.InputSystem;

namespace Kryz.RPG.Items.UGUI
{
	public class ItemSlotDragController : BaseItemSlotDragController
	{
		[SerializeField] Canvas? canvas;
		[SerializeField] BaseItemSlotView? cursorSlot;
		[SerializeField] bool copySourceSlotSize;

		private IItemSlotView? sourceSlot;

		public override IItemSlotView? CursorSlot => cursorSlot;
		public override IItemSlotView? SourceSlot => sourceSlot;

		private void OnValidate()
		{
			if (canvas == null) canvas = GetComponentInParent<Canvas>(includeInactive: true);
			if (cursorSlot == null) cursorSlot = GetComponentInChildren<BaseItemSlotView>(includeInactive: true);
		}

		private void LateUpdate()
		{
			if (cursorSlot != null)
			{
				bool enabled = cursorSlot.VisualItemStack != default;

				if (enabled)
				{
					SetScreenPosition(canvas, cursorSlot.RectTransform, Mouse.current.position.value);
				}

				if (cursorSlot.gameObject.activeSelf != enabled)
				{
					cursorSlot.gameObject.SetActive(enabled);
				}
			}
		}

		private static void SetScreenPosition(Canvas? canvas, RectTransform rectTransform, Vector2 screenPos)
		{
			Camera? cam = canvas != null ? canvas.worldCamera : null;
			RectTransform parent = (RectTransform)rectTransform.parent;

			if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPos, cam, out Vector2 localPos))
			{
				rectTransform.localPosition = localPos;
			}
		}

		public override bool StartDrag(IItemSlotView slotView)
		{
			if (cursorSlot == null || slotView == null)
				return false;

			if (copySourceSlotSize && slotView is BaseItemSlotView baseView)
			{
				Vector3 source = baseView.RectTransform.lossyScale;
				Vector3 target = cursorSlot.RectTransform.parent.lossyScale;

				if (source != target)
				{
					cursorSlot.RectTransform.localScale = new Vector3(source.x / target.x, source.y / target.y, source.z / target.z);
				}

				cursorSlot.RectTransform.sizeDelta = baseView.RectTransform.sizeDelta;
			}

			sourceSlot = slotView;
			return true;
		}

		public override bool StopDrag()
		{
			sourceSlot = null;
			return true;
		}
	}
}