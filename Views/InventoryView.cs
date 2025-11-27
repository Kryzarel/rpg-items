using System;
using System.Collections.Generic;
using Kryz.RPG.Items.Interfaces;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kryz.RPG.Items.Views
{
	[RequireComponent(typeof(RectTransform))]
	public class InventoryView : MonoBehaviour
	{
		[Header("Project References")]
		[SerializeField]
		private ItemSlotView? itemSlotPrefab;

		[Header("Child/Self References")]
		[SerializeField]
		private RectTransform? itemSlotsParent;
		[SerializeField]
		private List<ItemSlotView> itemSlots = new();
		[SerializeField]
		private Serialize<IItemSlotInteraction> itemSlotInteraction;

		[Header("Settings")]
		[SerializeField, Tooltip("The minimum amount of slots to instantiate and display.")]
		private int minSlots = 10;
		[SerializeField, Tooltip("The maximum amount of slots that can be displayed. A value of 0 or lower means infinite.")]
		private int maxSlots = 0;

		private Action<int>? onSlotChangedDelegate;
		private Action<int>? onNumSlotsChangedDelegate;
		private IInventory? inventory;

		public IInventory? Inventory => inventory;
		public IReadOnlyList<ItemSlotView> ItemSlots => itemSlots;

		private void OnValidate()
		{
			if (itemSlotsParent == null) itemSlotsParent = (RectTransform)transform;
			itemSlotsParent.GetComponentsInChildren(includeInactive: true, itemSlots);
#if UNITY_EDITOR
			if (!EditorApplication.isPlayingOrWillChangePlaymode) EditorApplication.delayCall += HandleSlotViews;
#endif
		}

		private void Awake()
		{
			OnValidate(); // Call OnValidate to make sure everything is setup correctly
			HandleSlotViews();
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

			if (inventory != null)
			{
				inventory.OnSlotChanged += onSlotChangedDelegate;
				inventory.OnNumSlotsChanged += onNumSlotsChangedDelegate;
			}

			HandleSlotViews();
		}

		private void HandleSlotViews()
		{
			if (inventory != null && inventory.NumSlots > maxSlots && maxSlots > 0)
			{
				Debug.LogError("Inventory has " + inventory.NumSlots + " slots, but the " + nameof(InventoryView) + " is configured to display a maximum of " + maxSlots, this);
			}

			int inventorySlots = inventory != null ? inventory.NumSlots : 0;
			int numSlots = Math.Min(Math.Max(inventorySlots, minSlots), maxSlots <= 0 ? int.MaxValue : maxSlots);

			for (int i = 0; i < itemSlots.Count; i++)
			{
				ItemSlotView itemSlotView = itemSlots[i];
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
					ItemSlotView itemSlotView = Instantiate(itemSlotPrefab, itemSlotsParent);
					itemSlots.Add(itemSlotView);
					itemSlotView.Setup(i, inventory, itemSlotInteraction.Value);
					itemSlotView.gameObject.SetActive(true);
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