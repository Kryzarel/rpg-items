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
			EndDrag();
		}

		private void OnDisable()
		{
			inventoryActionMap?.Disable();
		}

		private void StartDrag(IItemSlotView slotView, ItemStack itemStack = default)
		{
			dragManager.Value?.StartDrag(slotView, itemStack);

			// Pick Up Action should not be disabled because we need it to drop the picked up item
			if (pickUpAction != null) pickUpAction.action.Enable();
			if (cancelPickUpAction != null) cancelPickUpAction.action.Enable();

			if (useAction != null) useAction.action.Disable();
			if (splitStackAction != null) splitStackAction.action.Disable();
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
			if (dragManager.Value != null && pickUpAction != null && pickUpAction.action.WasPerformedThisFrame())
			{
				IItemSlotView? draggedSlot = dragManager.Value.DraggedSlot;
				bool wasEmpty = itemSlotView.ItemStack.Item == null;
				bool isSameItem = itemSlotView.ItemStack.Item == draggedSlot?.ItemStack.Item;

				if (draggedSlot != null)
				{
					if (draggedSlot == itemSlotView)
					{
						EndDrag();
						return true;
					}

					TryMove(itemSlotView, out ItemStack remainder);
					EndDrag();

					if (remainder != default)
					{
						StartDrag(draggedSlot, remainder);
						return true;
					}
				}

				if (!wasEmpty && !isSameItem && ValidateSlot(itemSlotView, out _, out _))
				{
					StartDrag(itemSlotView);
					return true;
				}
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
					ItemStack stack = itemSlotView.ItemStack;
					if (stack.TryRemove(stack.Count / 2, out ItemStack result, out ItemStack splitStack))
					{
						if (itemSlotView is BaseItemSlotView baseView) baseView.SetItemStack(result);
						StartDrag(itemSlotView, splitStack);
						return true;
					}
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
					ItemStack stack = itemSlotView.ItemStack;
					if (stack.TryRemove(stack.Count / 2, out ItemStack result, out ItemStack splitStack))
					{
						if (itemSlotView is BaseItemSlotView baseView) baseView.SetItemStack(result);
						StartDrag(itemSlotView, splitStack);
						return true;
					}
				}
				else
				{
					StartDrag(itemSlotView);
					return true;
				}
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
			return TryMove(itemSlotView, out _);
		}

		private bool TryMove(IItemSlotView itemSlotView, out ItemStack remainder)
		{
			remainder = default;

			if (!ValidateSlot(itemSlotView, out int dropIndex, out IInventory? dropInventory))
				return false;

			if (dragManager.Value == null || dragManager.Value.DraggedSlot == null)
				return false;

			if (!ValidateSlot(dragManager.Value.DraggedSlot, out int dragIndex, out IInventory? dragInventory))
				return false;

			ItemStack draggedStack = dragManager.Value.DraggedItemStack;
			return dropInventory.TryMove(dropIndex, dragIndex, dragInventory, out remainder, draggedStack.Count, allowSwap: true);
		}

		private static bool ValidateSlot(IItemSlotView itemSlotView, out int index, [MaybeNullWhen(returnValue: false)] out IInventory inventory)
		{
			index = itemSlotView.SlotIndex;
			inventory = itemSlotView.Inventory;
			return inventory != null && index >= 0 && index < inventory.Size;
		}
	}
}