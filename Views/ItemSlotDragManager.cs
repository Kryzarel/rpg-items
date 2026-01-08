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

		private IInventory.OnSlotChangedDelegate? onSlotChangedAction;
		private BaseItemSlotView? draggedSlot;

		public IItemSlotView? DraggedSlot => draggedSlot;
		public ItemStack DraggedItemStack => dragSlot != null ? dragSlot.ItemStack : default;

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

		public void StartDrag(IItemSlotView slotView, ItemStack itemStack = default)
		{
			if (dragSlot != null && slotView is BaseItemSlotView baseView && baseView.Inventory != null)
			{
				Vector3 sourceScale = baseView.RectTransform.lossyScale;
				Vector3 targetScale = dragSlot.RectTransform.parent.lossyScale;

				dragSlot.RectTransform.sizeDelta = baseView.RectTransform.sizeDelta;
				dragSlot.RectTransform.localScale = new Vector3(
					sourceScale.x / targetScale.x,
					sourceScale.y / targetScale.y,
					sourceScale.z / targetScale.z);

				draggedSlot = baseView;
				draggedSlot.SetDragVisuals(isDragging: true);

				if (itemStack == default)
				{
					draggedSlot.Inventory.OnSlotChanged += onSlotChangedAction;
					dragSlot.Bind(baseView.SlotIndex, baseView.Inventory);
				}
				else
				{
					dragSlot.SetItemStack(itemStack);
					draggedSlot.SetItemStack(draggedSlot.ItemStack.Remove(itemStack.Count, out _));
				}

				dragSlot.gameObject.SetActive(true);
			}
		}

		public void StopDrag()
		{
			if (draggedSlot != null)
			{
				if (draggedSlot.Inventory != null)
				{
					draggedSlot.Inventory.OnSlotChanged -= onSlotChangedAction;
					draggedSlot.RefreshBinding();
				}
				draggedSlot.SetDragVisuals(isDragging: false);
				draggedSlot = null;
			}

			if (dragSlot != null)
			{
				dragSlot.gameObject.SetActive(false);
				dragSlot.Unbind();
			}
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