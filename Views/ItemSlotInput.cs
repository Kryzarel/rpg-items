using System.Diagnostics.CodeAnalysis;
using Kryz.UnityUtils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Kryz.RPG.Items.Views
{
	public class ItemSlotInput : MonoBehaviour, IItemSlotInput
	{
		[Header("References")]
		[SerializeField] SerializeInterface<IItemTooltip> itemTooltip;
		[SerializeField] SerializeInterface<IItemSlotDragManager> dragManager;
		[Header("Input Actions")]
		[SerializeField] InputActionAsset? actionAsset;
		[SerializeField] InputActionReference? pickUpAction;
		[SerializeField] InputActionReference? cancelPickUpAction;
		[SerializeField] InputActionReference? useAction;
		[SerializeField] InputActionReference? splitStackAction;

		private InputActionMap? inventoryActionMap;

		private void Start()
		{
			if (actionAsset != null)
			{
				inventoryActionMap = actionAsset.FindActionMap("Inventory");
			}

			if (cancelPickUpAction != null)
			{
				cancelPickUpAction.action.performed += ctx => EndDrag();
			}
		}

		private void OnEnable()
		{
			inventoryActionMap?.Enable();

			if (pickUpAction != null) pickUpAction.action.Enable();
			if (cancelPickUpAction != null) cancelPickUpAction.action.Disable();

			if (useAction != null) useAction.action.Enable();
			if (splitStackAction != null) splitStackAction.action.Enable();
		}

		private void OnDisable()
		{
			inventoryActionMap?.Disable();
		}

		private bool StartDrag(IItemSlotView slotView, int count)
		{
			if (dragManager.Value == null || !dragManager.Value.StartDrag(slotView, count))
			{
				return false;
			}

			// Pick Up Action should not be disabled because we need it to drop the picked up item
			if (pickUpAction != null) pickUpAction.action.Enable();
			if (cancelPickUpAction != null) cancelPickUpAction.action.Enable();

			if (useAction != null) useAction.action.Disable();
			if (splitStackAction != null) splitStackAction.action.Disable();

			return true;
		}

		private void EndDrag()
		{
			dragManager.Value?.StopDrag();

			if (pickUpAction != null) pickUpAction.action.Enable();
			if (cancelPickUpAction != null) cancelPickUpAction.action.Disable();

			if (useAction != null) useAction.action.Enable();
			if (splitStackAction != null) splitStackAction.action.Enable();
		}

		public bool OnPointerClick(IItemSlotView itemSlotView, PointerEventData eventData)
		{
			if (HandleItemPickup(itemSlotView))
			{
				return true;
			}

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

		public bool OnPointerEnter(IItemSlotView itemSlotView, PointerEventData eventData)
		{
			if (ValidateSlot(itemSlotView, out _, out _))
			{
				itemTooltip.Value?.Show(itemSlotView.ItemStack, eventData);
				return true;
			}
			return false;
		}

		public bool OnPointerExit(IItemSlotView itemSlotView, PointerEventData eventData)
		{
			itemTooltip.Value?.Hide();
			return true;
		}

		public bool OnBeginDrag(IItemSlotView itemSlotView, PointerEventData eventData)
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

		private bool HandleItemPickup(IItemSlotView itemSlotView)
		{
			if (dragManager.Value == null || pickUpAction == null || !pickUpAction.action.WasPerformedThisFrame())
			{
				return false;
			}

			IItemSlotView? dragSlot = dragManager.Value.CursorSlot;
			ItemStack dragBefore = dragSlot?.ItemStack ?? default;
			ItemStack dropBefore = itemSlotView.ItemStack;

			if (TryDrop(itemSlotView))
			{
				ItemStack dragAfter = dragSlot?.ItemStack ?? default;
				ItemStack dropAfter = itemSlotView.ItemStack;
				bool notEmpty = dragBefore != default && dragAfter != default;
				bool swapped = notEmpty && dragBefore == dropAfter && dropBefore == dragAfter;

				EndDrag();

				if (swapped)
				{
					StartDrag(itemSlotView, itemSlotView.ItemStack.Count);
				}
				return true;
			}

			return StartDrag(itemSlotView, itemSlotView.ItemStack.Count);
		}

		public bool OnEndDrag(IItemSlotView itemSlotView, PointerEventData eventData)
		{
			EndDrag();
			return true;
		}

		public bool OnDrag(IItemSlotView itemSlotView, PointerEventData eventData)
		{
			return true;
		}

		public bool OnDrop(IItemSlotView itemSlotView, PointerEventData eventData)
		{
			return TryDrop(itemSlotView);
		}

		public bool OnSelect(IItemSlotView itemSlotView, BaseEventData eventData)
		{
			return false;
		}

		public bool OnDeselect(IItemSlotView itemSlotView, BaseEventData eventData)
		{
			return false;
		}

		private bool TryDrop(IItemSlotView itemSlotView)
		{
			if (!ValidateSlot(itemSlotView, out int dropIndex, out IInventory? dropInventory))
				return false;

			if (dragManager.Value == null || dragManager.Value.CursorSlot == null)
				return false;

			if (!ValidateSlot(dragManager.Value.CursorSlot, out int dragIndex, out IInventory? dragInventory))
				return false;

			return dropInventory.TryMove(dropIndex, dragInventory, dragIndex, allowSwap: true);
		}

		private static bool ValidateSlot(IItemSlotView itemSlotView, out int index, [MaybeNullWhen(returnValue: false)] out IInventory inventory)
		{
			index = itemSlotView.SlotIndex;
			inventory = itemSlotView.Inventory;
			return inventory != null && index >= 0 && index < inventory.Size;
		}
	}
}