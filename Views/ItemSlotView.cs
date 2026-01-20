using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kryz.RPG.Items.Views
{
	public class ItemSlotView : BaseItemSlotView, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler, ISelectHandler, IDeselectHandler
	{
		[Header("References")]
		[SerializeField] Image? raycastImage;
		[SerializeField] Image? itemImage;
		[SerializeField] TextMeshProUGUI? text;
		[SerializeField] Selectable? selectable;
		[Header("Settings")]
		[SerializeField] string textFormatString = "{0}";
		[SerializeField] Color emptyColor = Color.clear;
		[SerializeField] Color nonEmptyColor = Color.white;
		[SerializeField] Color disabledColor = Color.gray;
		[SerializeField] Sprite? disabledSprite;

		private int cachedCount;
		private int slotIndex = -1;
		private IInventory? inventory;
		private IItemSlotInput? input;
		private ItemStack itemStack;

		public override int SlotIndex => slotIndex;
		public override IInventory? Inventory => inventory;
		public override IItemSlotInput? Input => input;
		public override ItemStack ItemStack => itemStack;

		public bool IsActive => itemStack != default || inventory != null && slotIndex >= 0 && slotIndex < inventory.Size;

		private void OnValidate()
		{
			if (raycastImage == null) raycastImage = GetComponentInChildren<Image>(includeInactive: true);
			if (itemImage == null) itemImage = GetComponentInChildren<Image>(includeInactive: true);
			if (text == null) text = GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);
			if (selectable == null) selectable = GetComponentInChildren<Selectable>(includeInactive: true);

			if (raycastImage != null) raycastImage.raycastTarget = input != null;
			if (itemImage != null && itemImage != raycastImage) itemImage.raycastTarget = false;
			if (text != null) text.raycastTarget = false;

			RefreshVisuals();
		}

		public override void Bind(int slotIndex, IInventory inventory, IItemSlotInput? input = null)
		{
			if (this.slotIndex == slotIndex && this.inventory == inventory && this.input == input)
			{
				return;
			}

			this.slotIndex = slotIndex;
			this.inventory = inventory;
			this.input = input;

			if (inventory != null && slotIndex >= 0 && slotIndex < inventory.Size)
			{
				itemStack = inventory[slotIndex];
			}

			RefreshVisuals();
		}

		public override void Unbind()
		{
			slotIndex = -1;
			inventory = null;
			input = null;
			itemStack = default;
			RefreshVisuals();
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
			if (IsActive) RefreshActive();
			else RefreshInactive();
		}

		private void RefreshActive()
		{
			if (raycastImage != null)
			{
				raycastImage.raycastTarget = input != null;
			}

			if (itemImage != null)
			{
				if (itemStack.Item != null)
				{
					itemImage.sprite = itemStack.Item.Icon;
					itemImage.color = nonEmptyColor;
				}
				else
				{
					itemImage.sprite = null!;
					itemImage.color = emptyColor;
				}
			}

			if (text != null)
			{
				if (itemStack.Count > 1 && itemStack.Count != cachedCount)
				{
					text.text = string.Format(string.IsNullOrEmpty(textFormatString) ? "{0}" : textFormatString, itemStack.Count);
					cachedCount = itemStack.Count;
				}

				text.color = nonEmptyColor;
				text.enabled = itemStack.Count > 1;
			}

			if (selectable != null)
			{
				selectable.interactable = true;
			}
		}

		private void RefreshInactive()
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

			if (selectable != null)
			{
				selectable.interactable = false;
			}
		}

		public void OnPointerClick(PointerEventData eventData) => input?.OnPointerClick(this, eventData);
		public void OnPointerEnter(PointerEventData eventData) => input?.OnPointerEnter(this, eventData);
		public void OnPointerExit(PointerEventData eventData) => input?.OnPointerExit(this, eventData);
		public void OnBeginDrag(PointerEventData eventData) => input?.OnBeginDrag(this, eventData);
		public void OnEndDrag(PointerEventData eventData) => input?.OnEndDrag(this, eventData);
		public void OnDrag(PointerEventData eventData) => input?.OnDrag(this, eventData);
		public void OnDrop(PointerEventData eventData) => input?.OnDrop(this, eventData);
		public void OnSelect(BaseEventData eventData) => input?.OnSelect(this, eventData);
		public void OnDeselect(BaseEventData eventData) => input?.OnDeselect(this, eventData);
	}
}