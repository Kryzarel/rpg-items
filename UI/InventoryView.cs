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
		private List<ItemSlotView> itemSlots = new();
		[SerializeField]
		private Serialize<IItemSlotInteraction> itemSlotInteraction;

		[Header("Settings")]
		[SerializeField, Tooltip("The minimum amount of slots to instantiate and display.")]
		private int minSlots;
		[SerializeField, Tooltip("The maximum amount of slots that can be displayed. A value of 0 or lower means infinite.")]
		private int maxSlots;

		private Action<int> onSlotChangedDelegate;
		private Action<int> onNumSlotsChangedDelegate;
		private IInventory? inventory;

		public IInventory? Inventory => inventory;
		public IReadOnlyList<ItemSlotView> ItemSlots => itemSlots;

		private void OnValidate()
		{
			if (itemSlotsParent == null) itemSlotsParent = (RectTransform)transform;
			itemSlotsParent.GetComponentsInChildren(includeInactive: true, itemSlots);
			HandleSlotViews();
		}

		private void Awake()
		{
			OnValidate(); // Call OnValidate to make sure everything is setup correctly
			onSlotChangedDelegate = OnSlotChanged;
			onNumSlotsChangedDelegate = OnNumSlotsChanged;
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
			}
		}

		private void HandleSlotViews()
		{
			if (inventory != null && inventory.NumSlots > maxSlots && maxSlots > 0)
			{
				Debug.LogError($"Inventory has {inventory.NumSlots} slots, but the {nameof(InventoryView)} is configured to display a maximum of {maxSlots}.", this);
			}

			int inventorySlots = inventory != null ? inventory.NumSlots : 0;
			int numSlots = Math.Clamp(inventorySlots, minSlots, maxSlots);

			for (int i = 0; i < itemSlots.Count; i++)
			{
				ItemSlotView itemSlotView = itemSlots[i];
				itemSlotView.Setup(i, itemSlotInteraction.Value);
				itemSlotView.gameObject.SetActive(i < numSlots);
			}

			if (itemSlots.Count < numSlots && itemSlotPrefab == null)
			{
				string msg = $"Failed to instantiate {nameof(ItemSlotView)}s because {nameof(itemSlotPrefab)} is null.";
				if (Application.isPlaying) Debug.LogError(msg, this);
				else Debug.LogWarning(msg, this);
				return;
			}

			for (int i = itemSlots.Count; i < numSlots; i++)
			{
				ItemSlotView itemSlotView = Instantiate(itemSlotPrefab, itemSlotsParent);
				itemSlots.Add(itemSlotView);
				itemSlotView.Setup(i, itemSlotInteraction.Value);
				itemSlotView.gameObject.SetActive(true);
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