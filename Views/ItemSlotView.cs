using Kryz.RPG.Items.Interfaces;
using Kryz.RPG.Items.Structs;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kryz.RPG.Items.Views
{
	public class ItemSlotView : BaseItemSlotView, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler
	{
		[Header("References")]
		[SerializeField] Image? image;
		[SerializeField] TextMeshProUGUI? text;
		[Header("Settings")]
		[SerializeField] string textFormatString = string.Empty;
		[SerializeField] Color emptyColor = Color.clear;
		[SerializeField] Color nonEmptyColor = Color.white;

		private int index;
		private IInventory? inventory;
		private IItemSlotInteraction? interaction;
		private ItemStack itemStack;

		public override int Index => index;
		public override IInventory? Inventory => inventory;
		public IItemSlotInteraction? Interaction => interaction;
		public override ItemStack ItemStack => itemStack;

		private void OnValidate()
		{
			if (image == null) image = GetComponentInChildren<Image>(includeInactive: true);
			if (text == null) text = GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);

			if (image != null) image.raycastTarget = true;
			if (text != null) text.raycastTarget = false;
		}

		private void Awake()
		{
			OnValidate();
			SetItemStack(default, forceSet: true);
		}

		public override void Setup(int index, IInventory? inventory, IItemSlotInteraction? interaction)
		{
			this.index = index;
			this.inventory = inventory;
			this.interaction = interaction;

			if (image != null)
			{
				image.raycastTarget = inventory != null && index >= 0 && index < inventory.NumSlots;
			}
		}

		public override void SetItemStack(ItemStack stack)
		{
			SetItemStack(stack, forceSet: false);
		}

		private void SetItemStack(ItemStack stack, bool forceSet)
		{
			if (itemStack != stack || forceSet)
			{
				if (image != null)
				{
					image.sprite = stack.Item?.Icon!;
					image.color = stack.Item == null ? emptyColor : nonEmptyColor;
				}

				if (text != null && itemStack.Count != stack.Count)
				{
					bool enabled = stack.Count > 1;

					if (enabled)
					{
						text.text = string.Format(textFormatString, stack.Count);
					}

					if (text.enabled != enabled)
					{
						text.enabled = enabled;
					}
				}

				itemStack = stack;
			}
		}

		public void OnPointerClick(PointerEventData eventData) => interaction?.OnPointerClick(this, eventData);
		public void OnPointerEnter(PointerEventData eventData) => interaction?.OnPointerEnter(this, eventData);
		public void OnPointerExit(PointerEventData eventData) => interaction?.OnPointerExit(this, eventData);
		public void OnBeginDrag(PointerEventData eventData) => interaction?.OnBeginDrag(this, eventData);
		public void OnEndDrag(PointerEventData eventData) => interaction?.OnEndDrag(this, eventData);
		public void OnDrag(PointerEventData eventData) => interaction?.OnDrag(this, eventData);
		public void OnDrop(PointerEventData eventData) => interaction?.OnDrop(this, eventData);
	}
}