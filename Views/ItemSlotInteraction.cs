using System.Diagnostics.CodeAnalysis;
using Kryz.RPG.Items.Interfaces;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kryz.RPG.Items.Views
{
	public class ItemSlotInteraction : MonoBehaviour, IItemSlotInteraction
	{
		[SerializeField] Serialize<IItemTooltip> itemTooltip;
		[SerializeField] Serialize<IItemSlotDragManager> dragManager;

		public bool OnPointerClick(IItemSlotView itemSlotView, PointerEventData eventData)
		{
			if (ValidateSlot(itemSlotView, out int index, out IInventory? inventory))
			{
				if (eventData.button == PointerEventData.InputButton.Left)
				{
				}
				else if (eventData.button == PointerEventData.InputButton.Right)
				{
				}
				else if (eventData.button == PointerEventData.InputButton.Middle)
				{
				}
				return true;
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
			if (ValidateSlot(itemSlotView, out _, out _))
			{
				dragManager.Value?.OnDrag(itemSlotView, eventData);
				return true;
			}
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
			index = itemSlotView.Index;
			inventory = itemSlotView.Inventory;
			return inventory != null && index >= 0 && index < inventory.NumSlots;
		}
	}
}