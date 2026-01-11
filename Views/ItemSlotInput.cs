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
					return StartDrag(itemSlotView, itemSlotView.ItemStack.Count);
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
			if (dragManager.Value != null && pickUpAction != null && pickUpAction.action.WasPerformedThisFrame())
			{
				if (TryDrop(itemSlotView, out ItemStack remainder))
				{
					if (remainder == default)
					{
						EndDrag();
					}
					return true;
				}
				return StartDrag(itemSlotView, itemSlotView.ItemStack.Count);
			}
			return false;
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
			return TryDrop(itemSlotView, out _);
		}

		private bool TryDrop(IItemSlotView itemSlotView, out ItemStack remainder)
		{
			remainder = default;

			if (!ValidateSlot(itemSlotView, out int dropIndex, out IInventory? dropInventory))
				return false;

			if (dragManager.Value == null || dragManager.Value.SourceSlot == null || dragManager.Value.DragSlot == null)
				return false;

			if (!ValidateSlot(dragManager.Value.DragSlot, out int dragIndex, out IInventory? dragInventory))
				return false;

			if (!dragInventory.CanRemove(dragIndex, out _, out ItemStack dragStack))
				return false;

			if (dropInventory.CanAdd(dropIndex, dragStack, out _, out remainder, allowPartial: true) && dragInventory.CanSwap(dragIndex, remainder, out _))
			{
				dropInventory.TryAdd(dropIndex, dragStack, out _, out remainder, allowPartial: true);
				dragInventory.TrySwap(dragIndex, remainder, out ItemStack removed);
				if (remainder != default) remainder = removed;
				return true;
			}
			else if (dropInventory.CanSwap(dropIndex, dragStack, out ItemStack removed) && dragInventory.CanSwap(dragIndex, removed, out remainder))
			{
				dropInventory.TrySwap(dropIndex, dragStack, out removed);
				dragInventory.TrySwap(dragIndex, removed, out remainder);
				return true;
			}

			return false;
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