using System;
using System.Collections.Generic;
using Kryz.RPG.Items.Interfaces;
using UnityEngine;

namespace Kryz.RPG.Items.UI
{
	[RequireComponent(typeof(RectTransform))]
	public class InventoryView : MonoBehaviour
	{
		[Header("Project References")]
		[SerializeField]
		private ItemSlotView itemSlotPrefab;

		[Header("Child/Self References")]
		[SerializeField]
		private RectTransform itemSlotsParent;
		[SerializeField]
		private List<ItemSlotView> itemSlots;

		[Header("Settings")]
		[SerializeField, Tooltip("The minimum amount of slots to instantiate and display.")]
		private int minSlots;
		[SerializeField, Tooltip("The maximum amount of slots that can be displayed. A value of 0 or lower means infinite.")]
		private int maxSlots;

		private IInventory? inventory;
		private Action<int> onSlotChangedDelegate;
		private Action<int> onNumSlotsChangedDelegate;

		public IInventory? Inventory => inventory;

		private void OnValidate()
		{
			if (itemSlotsParent == null) itemSlotsParent = (RectTransform)transform;
			itemSlotsParent.GetComponentsInChildren(includeInactive: true, itemSlots);
			HandleSlotViews();
		}

		private void Awake()
		{
			onSlotChangedDelegate = OnSlotChanged;
			onNumSlotsChangedDelegate = OnNumSlotsChanged;
		}

		private void HandleSlotViews()
		{
			if (inventory != null && inventory.NumSlots > maxSlots && maxSlots > 0)
			{
				Debug.LogError($"Inventory has {inventory.NumSlots} slots, but the {nameof(InventoryView)} is configured to display a maximum of {maxSlots}.", this);
			}

			int inventorySlots = inventory != null ? inventory.NumSlots : 0;
			int numSlots = Math.Clamp(inventorySlots, minSlots, maxSlots);

			for (int i = itemSlots.Count; i < numSlots; i++)
			{
				itemSlots.Add(Instantiate(itemSlotPrefab));
			}

			for (int i = 0; i < itemSlots.Count; i++)
			{
				ItemSlotView itemSlotView = itemSlots[i];
				itemSlotView.Index = i;
				itemSlotView.gameObject.SetActive(i < numSlots);
			}
		}

		public void SetInventory(IInventory? newInventory)
		{
			if (inventory == newInventory)
			{
				return;
			}

			if (inventory != null)
			{
				inventory.OnSlotChanged -= onSlotChangedDelegate;
				inventory.OnNumSlotsChanged -= onNumSlotsChangedDelegate;
			}

			inventory = newInventory;

			HandleSlotViews();

			if (inventory != null)
			{
				inventory.OnSlotChanged += onSlotChangedDelegate;
				inventory.OnNumSlotsChanged += onNumSlotsChangedDelegate;

				for (int i = 0; i < inventory.Count; i++)
				{
					itemSlots[i].SetItemStack(inventory[i]);
				}
			}
		}

		private void OnSlotChanged(int index)
		{
			if (inventory == null || index < 0 || index >= inventory.Count)
			{
				Debug.LogError($"Error handling {nameof(IInventory.OnSlotChanged)} event. Inventory Size: {inventory?.NumSlots} | Slot Index: {index}", this);
				return;
			}

			itemSlots[index].SetItemStack(inventory[index]);
		}

		private void OnNumSlotsChanged(int numSlots)
		{
			HandleSlotViews();
		}
	}
}