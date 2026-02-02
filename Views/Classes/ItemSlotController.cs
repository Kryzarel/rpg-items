using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Kryz.RPG.Items.Views
{
	public class ItemSlotController : BaseItemSlotController
	{
		[Header("References")]
		[SerializeField] protected BaseItemSlotDragController? dragController;
		[Header("Input Actions")]
		[SerializeField] protected InputActionReference? useAction;
		[SerializeField] protected InputActionReference? splitStackAction;

		protected virtual void OnValidate()
		{
			if (dragController == null) dragController = FindAnyObjectByType<BaseItemSlotDragController>();
		}

		protected virtual void OnEnable()
		{
			if (useAction != null) useAction.action.Enable();
			if (splitStackAction != null) splitStackAction.action.Enable();
		}

		protected virtual void OnDisable()
		{
			if (useAction != null) useAction.action.Disable();
			if (splitStackAction != null) splitStackAction.action.Disable();
		}

		protected virtual bool StartDrag(IItemSlotView slotView, int count)
		{
			if (dragController == null || !dragController.StartDrag(slotView, count))
			{
				return false;
			}

			if (useAction != null) useAction.action.Disable();
			if (splitStackAction != null) splitStackAction.action.Disable();

			return true;
		}

		protected virtual void EndDrag()
		{
			if (dragController != null) dragController.StopDrag();

			if (useAction != null) useAction.action.Enable();
			if (splitStackAction != null) splitStackAction.action.Enable();
		}

		protected virtual bool TryDrop(IItemSlotView itemSlotView)
		{
			if (!ValidateSlot(itemSlotView, out int dropIndex, out IInventory? dropInventory))
				return false;

			if (dragController == null /* || dragController.CursorSlot == null */ || dragController.SourceSlot == null)
				return false;

			// if (!ValidateSlot(dragController.CursorSlot, out int cursorIndex, out IInventory? cursorInventory))
			// 	return false;

			if (!ValidateSlot(dragController.SourceSlot, out int sourceIndex, out IInventory? sourceInventory))
				return false;

			if (!dragController.StopDrag())
				return false;

			return dropInventory.TryMove(dropIndex, sourceInventory, sourceIndex, allowSwap: true);
		}

		protected static bool ValidateSlot(IItemSlotView itemSlotView, out int index, [MaybeNullWhen(returnValue: false)] out IInventory inventory)
		{
			index = itemSlotView.SlotIndex;
			inventory = itemSlotView.Inventory;
			return inventory != null && index >= 0 && index < inventory.Size;
		}

		public override bool OnPointerClick(IItemSlotView itemSlotView, PointerEventData eventData)
		{
			if (ValidateSlot(itemSlotView, out _, out _) && itemSlotView.ItemStack.Item != null)
			{
				if (useAction != null && useAction.action.WasPerformedThisFrame())
				{
					Debug.Log($"Item Used: {itemSlotView.ItemStack.Item.Name}");
					return true;
				}
			}
			return false;
		}

		public override bool OnPointerEnter(IItemSlotView itemSlotView, PointerEventData eventData)
		{
			return true;
		}

		public override bool OnPointerExit(IItemSlotView itemSlotView, PointerEventData eventData)
		{
			return true;
		}

		public override bool OnBeginDrag(IItemSlotView itemSlotView, PointerEventData eventData)
		{
			if (eventData.button == PointerEventData.InputButton.Left && ValidateSlot(itemSlotView, out _, out _))
			{
				if (splitStackAction != null && splitStackAction.action.IsPressed())
				{
					return StartDrag(itemSlotView, itemSlotView.ItemStack.Count / 2);
				}

				return StartDrag(itemSlotView, itemSlotView.ItemStack.Count);
			}
			return false;
		}

		public override bool OnEndDrag(IItemSlotView itemSlotView, PointerEventData eventData)
		{
			EndDrag();
			return true;
		}

		public override bool OnDrag(IItemSlotView itemSlotView, PointerEventData eventData)
		{
			return true;
		}

		public override bool OnDrop(IItemSlotView itemSlotView, PointerEventData eventData)
		{
			return TryDrop(itemSlotView);
		}

		public override bool OnSelect(IItemSlotView itemSlotView, BaseEventData eventData)
		{
			return true;
		}

		public override bool OnDeselect(IItemSlotView itemSlotView, BaseEventData eventData)
		{
			return true;
		}
	}
}