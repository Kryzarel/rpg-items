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
		[SerializeField] Image? raycastImage;
		[SerializeField] Image? itemImage;
		[SerializeField] TextMeshProUGUI? text;
		[Header("Settings")]
		[SerializeField] string textFormatString = string.Empty;
		[SerializeField] Color emptyColor = Color.clear;
		[SerializeField] Color nonEmptyColor = Color.white;
		[SerializeField] Color disabledColor = Color.gray;
		[SerializeField] Sprite? disabledSprite;

		private int lastCount = -1;

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
			if (raycastImage == null) raycastImage = GetComponentInChildren<Image>(includeInactive: true);
			if (itemImage == null) itemImage = GetComponentInChildren<Image>(includeInactive: true);
			if (text == null) text = GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);

			if (raycastImage != null) raycastImage.raycastTarget = true;
			if (itemImage != null && itemImage != raycastImage) itemImage.raycastTarget = false;
			if (text != null) text.raycastTarget = false;

			RefreshVisuals();
		}

		private void OnEnable()
		{
			RefreshVisuals();
		}

		private void OnDisable()
		{
			RefreshVisuals();
		}

		public override void Setup(int index, IInventory? inventory, IItemSlotInteraction? interaction)
		{
			this.index = index;
			this.inventory = inventory;
			this.interaction = interaction;

			if (inventory != null && index >= 0 && index < inventory.NumSlots)
			{
				SetItemStack(inventory[index]);
				enabled = true;
			}
			else
			{
				enabled = false;
			}
		}

		public override void SetItemStack(ItemStack stack)
		{
			if (itemStack != stack)
			{
				itemStack = stack;
				RefreshVisuals();
			}
		}

		private void RefreshVisuals()
		{
			if (enabled) RefreshEnabled();
			else RefreshDisabled();
		}

		private void RefreshEnabled()
		{
			if (raycastImage != null)
			{
				raycastImage.raycastTarget = true;
			}

			if (itemImage != null)
			{
				itemImage.sprite = itemStack.Item?.Icon!;
				itemImage.color = itemStack.Item == null ? emptyColor : nonEmptyColor;
			}

			if (text != null)
			{
				if (itemStack.Count > 1 && itemStack.Count != lastCount)
				{
					lastCount = itemStack.Count;
					text.text = string.Format(textFormatString, itemStack.Count);
				}
				text.enabled = itemStack.Count > 1;
			}
		}

		private void RefreshDisabled()
		{
			if (raycastImage != null)
			{
				raycastImage.raycastTarget = false;
			}

			if (itemImage != null)
			{
				itemImage.sprite = disabledSprite!;
				itemImage.color = disabledColor;
			}

			if (text != null)
			{
				text.enabled = false;
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