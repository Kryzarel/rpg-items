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
		// [SerializeField] InputActionReference? pickUpAction;
		// [SerializeField] InputActionReference? cancelPickUpAction;
		[SerializeField] protected InputActionReference? useAction;
		[SerializeField] protected InputActionReference? splitStackAction;

		protected virtual void OnValidate()
		{
			if (dragController == null) dragController = FindAnyObjectByType<BaseItemSlotDragController>();
		}

		// private void Start()
		// {
		// 	if (cancelPickUpAction != null)
		// 	{
		// 		cancelPickUpAction.action.performed += ctx => EndDrag();
		// 	}
		// }

		protected virtual void OnEnable()
		{
			if (useAction != null) useAction.action.Enable();
			if (splitStackAction != null) splitStackAction.action.Enable();
		}

		protected virtual void OnDisable()
		{
			// if (pickUpAction != null) pickUpAction.action.Disable();
			// if (cancelPickUpAction != null) cancelPickUpAction.action.Disable();
			if (useAction != null) useAction.action.Disable();
			if (splitStackAction != null) splitStackAction.action.Disable();
		}

		protected virtual bool StartDrag(IItemSlotView slotView, int count)
		{
			if (dragController == null || !dragController.StartDrag(slotView, count))
			{
				return false;
			}

			// // Pick Up Action should not be disabled because we need it to drop the picked up item
			// if (pickUpAction != null) pickUpAction.action.Enable();
			// if (cancelPickUpAction != null) cancelPickUpAction.action.Enable();

			if (useAction != null) useAction.action.Disable();
			if (splitStackAction != null) splitStackAction.action.Disable();

			return true;
		}

		protected virtual void EndDrag()
		{
			if (dragController != null) dragController.StopDrag();

			// if (pickUpAction != null) pickUpAction.action.Enable();
			// if (cancelPickUpAction != null) cancelPickUpAction.action.Disable();

			if (useAction != null) useAction.action.Enable();
			if (splitStackAction != null) splitStackAction.action.Enable();
		}

		protected virtual bool TryDrop(IItemSlotView itemSlotView)
		{
			if (!ValidateSlot(itemSlotView, out int dropIndex, out IInventory? dropInventory))
				return false;

			if (dragController == null || dragController.CursorSlot == null)
				return false;

			if (!ValidateSlot(dragController.CursorSlot, out int dragIndex, out IInventory? dragInventory))
				return false;

			return dropInventory.TryMove(dropIndex, dragInventory, dragIndex, allowSwap: true);
		}

		protected static bool ValidateSlot(IItemSlotView itemSlotView, out int index, [MaybeNullWhen(returnValue: false)] out IInventory inventory)
		{
			index = itemSlotView.SlotIndex;
			inventory = itemSlotView.Inventory;
			return inventory != null && index >= 0 && index < inventory.Size;
		}

		public override bool OnPointerClick(IItemSlotView itemSlotView, PointerEventData eventData)
		{
			// if (HandleItemPickup(itemSlotView))
			// {
			// 	return true;
			// }

			if (ValidateSlot(itemSlotView, out _, out _) && itemSlotView.ItemStack.Item != null)
			{
				if (useAction != null && useAction.action.WasPerformedThisFrame())
				{
					Debug.Log($"Item Used: {itemSlotView.ItemStack.Item.Name}");
					return true;
				}
				else if (splitStackAction != null && splitStackAction.action.WasPerformedThisFrame())
				{
					return StartDrag(itemSlotView, itemSlotView.ItemStack.Count / 2);
				}
			}
			return false;
		}

		public override bool OnPointerEnter(IItemSlotView itemSlotView, PointerEventData eventData)
		{
			// if (ValidateSlot(itemSlotView, out _, out _))
			// {
			// 	itemTooltip.Value?.Show(itemSlotView.ItemStack, eventData);
			// 	return true;
			// }
			return true;
		}

		public override bool OnPointerExit(IItemSlotView itemSlotView, PointerEventData eventData)
		{
			// itemTooltip.Value?.Hide();
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

		// private bool HandleItemPickup(IItemSlotView itemSlotView)
		// {
		// 	if (dragManager.Value == null || pickUpAction == null || !pickUpAction.action.WasPerformedThisFrame())
		// 	{
		// 		return false;
		// 	}

		// 	IItemSlotView? dragSlot = dragManager.Value.CursorSlot;
		// 	ItemStack dragBefore = dragSlot?.ItemStack ?? default;
		// 	ItemStack dropBefore = itemSlotView.ItemStack;

		// 	if (TryDrop(itemSlotView))
		// 	{
		// 		ItemStack dragAfter = dragSlot?.ItemStack ?? default;
		// 		ItemStack dropAfter = itemSlotView.ItemStack;
		// 		bool notEmpty = dragBefore != default && dragAfter != default;
		// 		bool swapped = notEmpty && dragBefore == dropAfter && dropBefore == dragAfter;

		// 		EndDrag();

		// 		if (swapped)
		// 		{
		// 			StartDrag(itemSlotView, itemSlotView.ItemStack.Count);
		// 		}
		// 		return true;
		// 	}

		// 	return StartDrag(itemSlotView, itemSlotView.ItemStack.Count);
		// }

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