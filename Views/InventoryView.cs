using System;
using System.Collections.Generic;
using Kryz.UnityUtils;
using UnityEngine;

namespace Kryz.RPG.Items.Views
{
	[RequireComponent(typeof(RectTransform))]
	public partial class InventoryView : MonoBehaviour
	{
		[Header("Project References")]
		[SerializeField]
		private BaseItemSlotView? itemSlotPrefab;

		[Header("Child/Self References")]
		[SerializeField]
		private RectTransform? itemSlotsParent;
		[SerializeField]
		private List<BaseItemSlotView> itemSlots = new();
		[SerializeField]
		private SerializeInterface<IItemSlotInteraction> itemSlotInteraction;

		[Header("Settings")]
		[SerializeField, Tooltip("The minimum amount of slots to instantiate and display.")]
		private int minSlots;
		[SerializeField, Tooltip("The maximum amount of slots that can be displayed. A value of 0 or lower means infinite.")]
		private int maxSlots;

		private IInventory.OnSlotChangedDelegate? onSlotChangedAction;
		private IInventory.OnSizeChangedDelegate? onSizeChangedAction;
		private IInventory? inventory;

		public IInventory? Inventory => inventory;
		public IReadOnlyList<BaseItemSlotView> ItemSlots => itemSlots;

		partial void OnValidateEditor();

		private void OnValidate()
		{
			OnValidateRuntime();
			OnValidateEditor();
		}

		private void OnValidateRuntime()
		{
			if (itemSlotsParent == null) itemSlotsParent = (RectTransform)transform;
			itemSlotsParent.GetComponentsInChildren(includeInactive: true, itemSlots);
		}

		private void Awake()
		{
			onSlotChangedAction = OnSlotChanged;
			onSizeChangedAction = OnSizeChanged;
			OnValidateRuntime();
			HandleSlotViews();
		}

		public void SetInventory(IInventory? newInventory)
		{
			if (inventory == newInventory)
			{
				return;
			}

			if (inventory != null)
			{
				inventory.OnSlotChanged -= onSlotChangedAction;
				inventory.OnSizeChanged -= onSizeChangedAction;
			}

			inventory = newInventory;

			if (inventory != null)
			{
				inventory.OnSlotChanged += onSlotChangedAction;
				inventory.OnSizeChanged += onSizeChangedAction;
			}

			HandleSlotViews();
		}

		private void HandleSlotViews()
		{
			int maxSlots = this.maxSlots > 0 ? this.maxSlots : int.MaxValue;
			int inventorySlots = inventory != null ? inventory.Size : 0;
			int numSlots = Math.Min(Math.Max(inventorySlots, minSlots), maxSlots);

			if (inventory != null && inventory.Size > maxSlots)
			{
				Debug.LogError("Inventory has " + inventory.Size + " slots, but the " + nameof(InventoryView) + " is configured to display a maximum of " + maxSlots, this);
			}

			for (int i = 0; i < itemSlots.Count; i++)
			{
				BaseItemSlotView itemSlotView = itemSlots[i];
				itemSlotView.Setup(i, inventory, itemSlotInteraction.Value);
				itemSlotView.gameObject.SetActive(i < numSlots);
			}

			if (itemSlots.Count < numSlots)
			{
				if (itemSlotPrefab == null)
				{
					Debug.LogError("Failed to instantiate " + nameof(ItemSlotView) + "s because " + nameof(itemSlotPrefab) + " is null", this);
					return;
				}

				for (int i = itemSlots.Count; i < numSlots; i++)
				{
					BaseItemSlotView itemSlotView = Instantiate(itemSlotPrefab, itemSlotsParent);
					itemSlots.Add(itemSlotView);
					itemSlotView.Setup(i, inventory, itemSlotInteraction.Value);
					itemSlotView.gameObject.SetActive(true);
				}
			}
		}

		private void OnSlotChanged(int index, ItemStack previousStack, ItemStack newStack)
		{
			if (inventory == null || index < 0 || index >= inventory.Size)
			{
				Debug.LogError($"Error handling {nameof(IInventory.OnSlotChanged)} event. Slot Index: {index} | Inventory Size: {inventory?.Size}", this);
				return;
			}

			itemSlots[index].SetItemStack(newStack);
		}

		private void OnSizeChanged(int previousSize, int newSize)
		{
			HandleSlotViews();
		}
	}
}