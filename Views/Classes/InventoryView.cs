using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kryz.RPG.Items.Views
{
	[RequireComponent(typeof(RectTransform))]
	public partial class InventoryView : MonoBehaviour, IInventoryView
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
		private BaseItemSlotController? itemSlotController;

		[Header("Settings")]
		[SerializeField, Tooltip("The minimum number of slots to display. If the assigned " + nameof(IInventory) + " is smaller, extra slots are shown but disabled.")]
		private int minSlots;
		[SerializeField, Tooltip("The maximum number of slots that can be displayed. A value of 0 or negative means unlimited.\nIf the assigned " + nameof(IInventory) + " is larger, slots beyond the maximum cannot be accessed and an error is logged to the console, but other slots will still function.")]
		private int maxSlots;

		private IInventory.OnSlotChangedDelegate? onSlotChangedAction;
		private IInventory.OnSizeChangedDelegate? onSizeChangedAction;
		private IInventory? inventory;
		private IItemSlotController? controller;

		public IInventory? Inventory { get => inventory; set => SetInventory(value); }
		public IReadOnlyList<BaseItemSlotView> ItemSlots => itemSlots;

		IReadOnlyList<IItemSlotView> IInventoryView.ItemSlots => ItemSlots;

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
			if (itemSlotController == null) itemSlotController = FindAnyObjectByType<BaseItemSlotController>();
		}

		private void Awake()
		{
			onSlotChangedAction = OnSlotChanged;
			onSizeChangedAction = OnSizeChanged;
			controller = itemSlotController;
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

		public void SetItemSlotController(IItemSlotController? controller)
		{
			if (this.controller == controller)
			{
				return;
			}

			this.controller = controller;
			itemSlotController = controller as BaseItemSlotController;

			for (int i = 0; i < itemSlots.Count; i++)
			{
				BaseItemSlotView itemSlotView = itemSlots[i];
				if (inventory != null)
				{
					itemSlotView.Bind(i, inventory, controller);
				}
				else
				{
					itemSlotView.Unbind();
				}
			}
		}

		private void HandleSlotViews()
		{
			int inventorySlots = inventory != null ? inventory.Size : 0;
			int maxSlots = this.maxSlots > 0 ? this.maxSlots : int.MaxValue;
			int numSlots = Math.Min(Math.Max(inventorySlots, minSlots), maxSlots);

			if (inventory != null && inventory.Size > maxSlots)
			{
				Debug.LogError("Inventory has " + inventory.Size + " slots, but the " + nameof(InventoryView) + " is configured to display a maximum of " + maxSlots, this);
			}

			if (itemSlots.Count < numSlots)
			{
				if (itemSlotPrefab == null)
				{
					Debug.LogError("Failed to instantiate " + nameof(ItemSlotView) + "s because " + nameof(itemSlotPrefab) + " is null", this);
				}
				else
				{
					for (int i = itemSlots.Count; i < numSlots; i++)
					{
						itemSlots.Add(Instantiate(itemSlotPrefab, itemSlotsParent));
					}
				}
			}

			for (int i = 0; i < itemSlots.Count; i++)
			{
				BaseItemSlotView itemSlotView = itemSlots[i];
				if (inventory != null)
				{
					itemSlotView.Bind(i, inventory, controller);
				}
				else
				{
					itemSlotView.Unbind();
				}
				itemSlotView.gameObject.SetActive(i < numSlots);
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