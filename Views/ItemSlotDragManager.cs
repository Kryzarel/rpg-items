using UnityEngine;
using UnityEngine.InputSystem;

namespace Kryz.RPG.Items.Views
{
	public class ItemSlotDragManager : MonoBehaviour, IItemSlotDragManager
	{
		[SerializeField]
		private Canvas? canvas;
		[SerializeField]
		private BaseItemSlotView? cursorSlot;

		private IItemSlotView? sourceSlot;

		public IItemSlotView? CursorSlot => cursorSlot;
		public IItemSlotView? SourceSlot => sourceSlot;

		private void OnValidate()
		{
			if (canvas == null) canvas = GetComponentInParent<Canvas>(includeInactive: true);
			if (cursorSlot == null) cursorSlot = GetComponentInChildren<BaseItemSlotView>(includeInactive: true);
		}

		private void Start()
		{
			if (cursorSlot != null)
			{
				cursorSlot.gameObject.SetActive(false);
			}
		}

		public bool StartDrag(IItemSlotView slotView, int count)
		{
			if (slotView.Inventory == null || cursorSlot == null || cursorSlot.Inventory == null)
			{
				return false;
			}

			if (!slotView.Inventory.CanRemove(slotView.SlotIndex, count, out _, out ItemStack removed) || !cursorSlot.Inventory.CanAdd(removed, out _))
			{
				return false;
			}

			sourceSlot = slotView;
			sourceSlot.Inventory.TryRemove(sourceSlot.SlotIndex, count, out _, out _);
			cursorSlot.Inventory.TryAdd(removed, out _);

			if (slotView is BaseItemSlotView baseView)
			{
				Vector3 source = baseView.RectTransform.lossyScale;
				Vector3 target = cursorSlot.RectTransform.parent.lossyScale;

				if (source != target)
				{
					cursorSlot.RectTransform.localScale = new Vector3(source.x / target.x, source.y / target.y, source.z / target.z);
				}

				cursorSlot.RectTransform.sizeDelta = baseView.RectTransform.sizeDelta;
			}

			cursorSlot.gameObject.SetActive(true);
			return true;
		}

		public bool StopDrag()
		{
			if (cursorSlot == null || sourceSlot == null)
			{
				return false;
			}

			ReturnCursorItem(cursorSlot, sourceSlot);
			cursorSlot.gameObject.SetActive(false);
			sourceSlot = null;
			return true;
		}

		private void LateUpdate()
		{
			if (cursorSlot != null)
			{
				bool enabled = cursorSlot.ItemStack != default;

				if (enabled)
				{
					SetScreenPosition(cursorSlot.RectTransform, Mouse.current.position.value);
				}

				if (cursorSlot.gameObject.activeSelf != enabled)
				{
					cursorSlot.gameObject.SetActive(enabled);
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

		private static void ReturnCursorItem(IItemSlotView dragSlot, IItemSlotView sourceSlot)
		{
			if (dragSlot.Inventory == null || dragSlot.Inventory[dragSlot.SlotIndex] == default || sourceSlot.Inventory == null)
				return;

			if (!dragSlot.Inventory.CanRemove(dragSlot.SlotIndex, out _, out ItemStack removed))
				return;

			if (!sourceSlot.Inventory.CanAdd(sourceSlot.SlotIndex, removed, out _, out _) && !sourceSlot.Inventory.CanAdd(removed, out _))
				return;

			if (sourceSlot.Inventory.TryAdd(sourceSlot.SlotIndex, removed, out _, out _) || sourceSlot.Inventory.TryAdd(removed, out _))
			{
				dragSlot.Inventory.TryRemove(dragSlot.SlotIndex, out _, out _);
			}
		}
	}
}