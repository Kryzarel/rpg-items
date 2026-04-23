using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kryz.RPG.Items.UGUI
{
	[RequireComponent(typeof(RectTransform))]
	public partial class ItemContainerView : MonoBehaviour, IItemContainerView
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
		[SerializeField, Tooltip("The minimum number of slots to display. If the assigned " + nameof(IItemContainer) + " is smaller, extra slots are shown but disabled.")]
		private int minSlots;
		[SerializeField, Tooltip("The maximum number of slots that can be displayed. A value of 0 or negative means unlimited.\nIf the assigned " + nameof(IItemContainer) + " is larger, slots beyond the maximum cannot be accessed and an error is logged to the console, but other slots will still function.")]
		private int maxSlots;

		private IItemContainerTransactional.OnSlotChangedDelegate? onSlotChangedAction;
		private IItemContainerTransactional.OnSizeChangedDelegate? onSizeChangedAction;
		private IItemContainer? itemContainer;
		private IItemSlotController? controller;

		public IItemContainer? ItemContainer { get => itemContainer; set => SetItemContainer(value); }
		public IReadOnlyList<BaseItemSlotView> ItemSlots => itemSlots;

		IReadOnlyList<IItemSlotView> IItemContainerView.ItemSlots => ItemSlots;

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
			controller = itemSlotController;
			HandleSlotViews();
		}

		public void SetItemContainer(IItemContainer? newContainer)
		{
			if (itemContainer == newContainer)
			{
				return;
			}

			if (itemContainer != null)
			{
				itemContainer.OnSlotChanged -= onSlotChangedAction;
				itemContainer.OnSizeChanged -= onSizeChangedAction;
			}

			itemContainer = newContainer;

			if (itemContainer != null)
			{
				itemContainer.OnSlotChanged += onSlotChangedAction;
				itemContainer.OnSizeChanged += onSizeChangedAction;
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
				if (itemContainer != null)
				{
					itemSlotView.Bind(i, itemContainer, controller);
				}
				else
				{
					itemSlotView.Unbind();
				}
			}
		}

		private void HandleSlotViews()
		{
			int containerSlots = itemContainer != null ? itemContainer.Size : 0;
			int maxSlots = this.maxSlots > 0 ? this.maxSlots : int.MaxValue;
			int numSlots = Math.Min(Math.Max(containerSlots, minSlots), maxSlots);

			if (itemContainer != null && itemContainer.Size > maxSlots)
			{
				Debug.LogError(nameof(IItemContainer) + " has " + itemContainer.Size + " slots, but the " + nameof(ItemContainerView) + " is configured to display a maximum of " + maxSlots, this);
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
				if (itemContainer != null)
				{
					itemSlotView.Bind(i, itemContainer, controller);
				}
				else
				{
					itemSlotView.Unbind();
				}
				itemSlotView.gameObject.SetActive(i < numSlots);
			}
		}

		private void OnSlotChanged(IItemContainerTransactional sender, int index, ItemStack previousStack, ItemStack newStack)
		{
			if (itemContainer == null || index < 0 || index >= itemContainer.Size)
			{
				Debug.LogError($"Error handling {nameof(IItemContainer.OnSlotChanged)} event. Slot Index: {index} | Container Size: {itemContainer?.Size}", this);
				return;
			}

			itemSlots[index].SetItemStack(newStack);
		}

		private void OnSizeChanged(IItemContainerTransactional sender, int previousSize, int newSize)
		{
			HandleSlotViews();
		}
	}
}