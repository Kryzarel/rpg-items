using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kryz.RPG.Items.UGUI
{
	public class ItemSlotView : BaseItemSlotView, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler, ISelectHandler, IDeselectHandler
	{
		[Header("References")]
		[SerializeField] protected Image? raycastImage;
		[SerializeField] protected Image? itemImage;
		[SerializeField] protected TextMeshProUGUI? text;
		[SerializeField] protected Selectable? selectable;
		[Header("Settings")]
		[SerializeField] protected string textFormatString = "{0}";
		[SerializeField] protected Sprite? emptySprite;
		[SerializeField] protected Color emptyColor = Color.clear;
		[SerializeField] protected Color nonEmptyColor = Color.white;
		[SerializeField] protected Sprite? unboundSprite;
		[SerializeField] protected Color unboundColor = Color.gray;

		protected int cachedCount;
		protected int slotIndex = -1;
		protected IItemContainer? itemContainer;
		protected IItemSlotController? controller;
		protected ItemStack itemStack;

		public override int SlotIndex => slotIndex;
		public override IItemContainer? ItemContainer => itemContainer;
		public override IItemSlotController? Controller => controller;
		public override ItemStack VisualItemStack => itemStack;

		public bool IsBound => itemStack != default || itemContainer != null && slotIndex >= 0 && slotIndex < itemContainer.Size;

		private void OnValidate()
		{
			if (raycastImage == null) raycastImage = GetComponentInChildren<Image>(includeInactive: true);
			if (itemImage == null) itemImage = GetComponentInChildren<Image>(includeInactive: true);
			if (text == null) text = GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);
			if (selectable == null) selectable = GetComponentInChildren<Selectable>(includeInactive: true);

			if (raycastImage != null) raycastImage.raycastTarget = controller != null;
			if (itemImage != null && itemImage != raycastImage) itemImage.raycastTarget = false;
			if (text != null) text.raycastTarget = false;

			RefreshVisuals();
		}

		public override void Bind(int slotIndex, IItemContainer itemContainer, IItemSlotController? controller = null)
		{
			this.slotIndex = slotIndex;
			this.itemContainer = itemContainer;
			this.controller = controller;

			if (itemContainer != null && slotIndex >= 0 && slotIndex < itemContainer.Size)
			{
				itemStack = itemContainer[slotIndex];
			}

			RefreshVisuals();
		}

		public override void Unbind()
		{
			slotIndex = -1;
			itemContainer = null;
			controller = null;
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

		protected void RefreshVisuals()
		{
			if (IsBound) RefreshBound();
			else RefreshUnbound();
		}

		protected virtual void RefreshBound()
		{
			if (raycastImage != null)
			{
				raycastImage.raycastTarget = controller != null;
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
					itemImage.sprite = emptySprite!;
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

				text.enabled = itemStack.Count > 1;
			}

			if (selectable != null)
			{
				selectable.interactable = controller != null;
			}
		}

		protected virtual void RefreshUnbound()
		{
			if (raycastImage != null)
			{
				raycastImage.raycastTarget = false;
			}

			if (itemImage != null)
			{
				itemImage.sprite = unboundSprite!;
				itemImage.color = unboundColor;
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

		public void OnPointerClick(PointerEventData eventData) => controller?.OnPointerClick(this, eventData);
		public void OnPointerEnter(PointerEventData eventData) => controller?.OnPointerEnter(this, eventData);
		public void OnPointerExit(PointerEventData eventData) => controller?.OnPointerExit(this, eventData);
		public void OnBeginDrag(PointerEventData eventData) => controller?.OnBeginDrag(this, eventData);
		public void OnEndDrag(PointerEventData eventData) => controller?.OnEndDrag(this, eventData);
		public void OnDrag(PointerEventData eventData) => controller?.OnDrag(this, eventData);
		public void OnDrop(PointerEventData eventData) => controller?.OnDrop(this, eventData);
		public void OnSelect(BaseEventData eventData) => controller?.OnSelect(this, eventData);
		public void OnDeselect(BaseEventData eventData) => controller?.OnDeselect(this, eventData);
	}
}