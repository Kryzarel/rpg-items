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
		[Header("Key Bindings")]
		[SerializeField] InputActionReference? useInputAction;
		[SerializeField] InputActionReference? splitStackInputAction;

		private void OnEnable()
		{
			if (useInputAction != null) useInputAction.action.Enable();
			if (splitStackInputAction != null) splitStackInputAction.action.Enable();
		}

		private void OnDisable()
		{
			if (useInputAction != null) useInputAction.action.Disable();
			if (splitStackInputAction != null) splitStackInputAction.action.Disable();
		}

		public bool OnPointerClick(IItemSlotView itemSlotView, PointerEventData eventData)
		{
			if (ValidateSlot(itemSlotView, out int index, out IInventory? inventory) && itemSlotView.ItemStack.Item != null)
			{
				if (useInputAction != null && useInputAction.action.WasPerformedThisFrame())
				{
					Debug.Log($"Item Used: {itemSlotView.ItemStack.Item.Name}");
					return true;
				}
				else if (splitStackInputAction != null && splitStackInputAction.action.WasPerformedThisFrame())
				{
					Debug.Log($"Item Split: {itemSlotView.ItemStack.Item.Name}");
					return true;
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
			if (ValidateSlot(itemSlotView, out _, out _))
			{
				dragManager.Value?.StartDrag(itemSlotView, eventData);
				return true;
			}
			return false;
		}

		public bool OnEndDrag(IItemSlotView itemSlotView, PointerEventData eventData)
		{
			dragManager.Value?.StopDrag(itemSlotView, eventData);
			return true;
		}

		public bool OnDrag(IItemSlotView itemSlotView, PointerEventData eventData)
		{
			// if (ValidateSlot(itemSlotView, out _, out _))
			// {
			// 	dragManager.Value?.OnDrag(itemSlotView, eventData);
			// 	return true;
			// }
			return false;
		}

		public bool OnDrop(IItemSlotView itemSlotView, PointerEventData eventData)
		{
			if (!ValidateSlot(itemSlotView, out int dropIndex, out IInventory? dropInventory))
				return false;

			if (!eventData.pointerDrag.TryGetComponent(out IItemSlotView dragView))
				return false;

			if (!ValidateSlot(dragView, out int dragIndex, out IInventory? dragInventory))
				return false;

			dropInventory.TryMove(dropIndex, dragIndex, dragInventory, allowSwap: true);
			return true;
		}

		private static bool ValidateSlot(IItemSlotView itemSlotView, out int index, [MaybeNullWhen(returnValue: false)] out IInventory inventory)
		{
			index = itemSlotView.SlotIndex;
			inventory = itemSlotView.Inventory;
			return inventory != null && index >= 0 && index < inventory.Size;
		}
	}
}