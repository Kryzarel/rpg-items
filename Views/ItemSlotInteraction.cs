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

		public void OnPointerClick(IItemSlotView itemSlotView, PointerEventData eventData)
		{
			if (ValidateSlot(itemSlotView, out int index, out IInventory? inventory))
			{
				if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)) && eventData.button == PointerEventData.InputButton.Left)
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

		public void OnPointerEnter(IItemSlotView itemSlotView, PointerEventData eventData)
		{
			if (ValidateSlot(itemSlotView, out _, out _))
			{
				itemTooltip.Value?.Show(itemSlotView.ItemStack, eventData);
			}
		}

		public void OnPointerExit(IItemSlotView itemSlotView, PointerEventData eventData)
		{
			itemTooltip.Value?.Hide();
		}

		public void OnBeginDrag(IItemSlotView itemSlotView, PointerEventData eventData)
		{
			if (ValidateSlot(itemSlotView, out _, out _))
			{
				dragManager.Value?.StartDrag(itemSlotView, eventData);
			}
		}

		public void OnEndDrag(IItemSlotView itemSlotView, PointerEventData eventData)
		{
			dragManager.Value?.StopDrag(itemSlotView, eventData);
		}

		public void OnDrag(IItemSlotView itemSlotView, PointerEventData eventData)
		{
			if (ValidateSlot(itemSlotView, out _, out _))
			{
				dragManager.Value?.OnDrag(itemSlotView, eventData);
			}
		}

		public void OnDrop(IItemSlotView itemSlotView, PointerEventData eventData)
		{
			if (!ValidateSlot(itemSlotView, out int dropIndex, out IInventory? dropInventory))
				return;

			if (!eventData.pointerDrag.TryGetComponent(out IItemSlotView dragView))
				return;

			if (!ValidateSlot(dragView, out int dragIndex, out IInventory? dragInventory))
				return;

			dropInventory.TryMove(dropIndex, dragIndex, dragInventory, allowSwap: true);
		}

		private static bool ValidateSlot(IItemSlotView itemSlotView, out int index, [MaybeNullWhen(returnValue: false)] out IInventory inventory)
		{
			index = itemSlotView.Index;
			inventory = itemSlotView.Inventory;
			return inventory != null && index >= 0 && index < inventory.NumSlots;
		}
	}
}