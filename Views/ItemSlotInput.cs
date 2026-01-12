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
			if (dragManager.Value == null || slotView.Inventory == null || !slotView.Inventory.TryRemove(slotView.SlotIndex, count, out _, out ItemStack removed))
			{
				return false;
			}

			dragManager.Value.StartDrag(slotView, removed);

			// Pick Up Action should not be disabled because we need it to drop the picked up item
			if (pickUpAction != null) pickUpAction.action.Enable();
			if (cancelPickUpAction != null) cancelPickUpAction.action.Enable();

			if (useAction != null) useAction.action.Disable();
			if (splitStackAction != null) splitStackAction.action.Disable();

			return true;
		}

		private void EndDrag()
		{
			if (dragManager.Value == null || dragManager.Value.DragSlot == null || dragManager.Value.SourceSlot == null)
			{
				return;
			}

			ReturnCursorItem(dragManager.Value.DragSlot, dragManager.Value.SourceSlot);
			dragManager.Value.StopDrag();

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
					StartDrag(itemSlotView, itemSlotView.ItemStack.Count / 2);
					return true;
				}

				StartDrag(itemSlotView, itemSlotView.ItemStack.Count);
				return true;
			}
			return false;
		}

		private bool HandleItemPickup(IItemSlotView itemSlotView)
		{
			if (dragManager.Value == null || pickUpAction == null || !pickUpAction.action.WasPerformedThisFrame())
			{
				return false;
			}

			if (TryDrop(itemSlotView))
			{
				ItemStack dragStack = dragManager.Value.DragSlot?.ItemStack ?? default;
				bool added = ValidateSlot(itemSlotView, out int index, out IInventory? inventory) && inventory.CanAdd(index, dragStack, out _, out _, allowPartial: true);
				EndDrag();
				return added || dragStack == default || StartDrag(itemSlotView, itemSlotView.ItemStack.Count);
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

		private bool TryDrop(IItemSlotView itemSlotView)
		{
			if (!ValidateSlot(itemSlotView, out int dropIndex, out IInventory? dropInventory))
				return false;

			if (dragManager.Value == null || dragManager.Value.SourceSlot == null || dragManager.Value.DragSlot == null)
				return false;

			if (!ValidateSlot(dragManager.Value.DragSlot, out int dragIndex, out IInventory? dragInventory))
				return false;

			return dropInventory.TryMove(dropIndex, dragInventory, dragIndex, allowSwap: true);
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

		private static bool ValidateSlot(IItemSlotView itemSlotView, out int index, [MaybeNullWhen(returnValue: false)] out IInventory inventory)
		{
			index = itemSlotView.SlotIndex;
			inventory = itemSlotView.Inventory;
			return inventory != null && index >= 0 && index < inventory.Size;
		}
	}
}