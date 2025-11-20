using System.Diagnostics.CodeAnalysis;
using Kryz.RPG.Items.Interfaces;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kryz.RPG.Items.UI
{
	public class ItemSlotInteraction : MonoBehaviour, IItemSlotInteraction
	{
		[SerializeField] InventoryView inventoryView;

		private void OnValidate()
		{
			if (inventoryView == null) inventoryView = GetComponent<InventoryView>();
		}

		public void OnPointerClick(ItemSlotView itemSlotView, PointerEventData eventData)
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
			}
		}

		public void OnPointerEnter(ItemSlotView itemSlotView, PointerEventData eventData)
		{
			if (ValidateSlot(itemSlotView, out int index, out IInventory? inventory))
			{
			}
		}

		public void OnPointerExit(ItemSlotView itemSlotView, PointerEventData eventData)
		{

		}

		public void OnBeginDrag(ItemSlotView itemSlotView, PointerEventData eventData)
		{
			if (ValidateSlot(itemSlotView, out int index, out IInventory? inventory))
			{
			}
		}

		public void OnEndDrag(ItemSlotView itemSlotView, PointerEventData eventData)
		{

		}

		public void OnDrag(ItemSlotView itemSlotView, PointerEventData eventData)
		{
			if (ValidateSlot(itemSlotView, out int index, out IInventory? inventory))
			{
			}
		}

		public void OnDrop(ItemSlotView itemSlotView, PointerEventData eventData)
		{
			if (!ValidateSlot(itemSlotView, out int index, out IInventory? inventory))
				return;

			if (!eventData.pointerDrag.TryGetComponent(out ItemSlotView draggedView))
				return;

			if (!ValidateSlot(draggedView, out int draggedIndex, out IInventory? draggedInventory))
				return;

			inventory.TryMove(index, draggedIndex, draggedInventory, allowSwap: true);
		}

		private bool ValidateSlot(ItemSlotView itemSlotView, out int index, [MaybeNullWhen(returnValue: false)] out IInventory inventory)
		{
			index = itemSlotView.Index;
			inventory = inventoryView.Inventory;
			return inventory != null && index > 0 && index < inventory.NumSlots;
		}
	}
}