using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kryz.RPG.Items.Views
{
	[ExecuteAlways]
	public class ItemSlotView : BaseItemSlotView, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler
	{
		[Header("References")]
		[SerializeField] Image? raycastImage;
		[SerializeField] Image? itemImage;
		[SerializeField] TextMeshProUGUI? text;
		[Header("Settings")]
		[SerializeField] string textFormatString = "{0}";
		[SerializeField] Color emptyColor = Color.clear;
		[SerializeField] Color nonEmptyColor = Color.white;
		[SerializeField] Color disabledColor = Color.gray;
		[SerializeField] Color draggingColor = Color.gray;
		[SerializeField] Sprite? disabledSprite;

		private int index;
		private IInventory? inventory;
		private IItemSlotInput? input;
		private ItemStack itemStack;
		private int lastCount = -1;
		private bool isDragging;

		public override int Index => index;
		public override IInventory? Inventory => inventory;
		public override IItemSlotInput? Input => input;
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

		public override void Setup(int index, IInventory? inventory, IItemSlotInput? input)
		{
			this.index = index;
			this.inventory = inventory;
			this.input = input;

			if (inventory != null && index >= 0 && index < inventory.Size)
			{
				enabled = true;
				SetItemStack(inventory[index]);
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
				raycastImage.raycastTarget = input != null;
			}

			if (itemImage != null)
			{
				if (itemStack.Item == null)
				{
					itemImage.sprite = null!;
					itemImage.color = emptyColor;
				}
				else
				{
					itemImage.sprite = itemStack.Item.Icon;
					itemImage.color = isDragging ? draggingColor : nonEmptyColor;
				}
			}

			if (text != null)
			{
				if (itemStack.Count > 1 && itemStack.Count != lastCount)
				{
					lastCount = itemStack.Count;

					if (string.IsNullOrEmpty(textFormatString))
					{
						text.text = itemStack.Count.ToString();
					}
					else
					{
						text.text = string.Format(textFormatString, itemStack.Count);
					}
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

		public void OnPointerClick(PointerEventData eventData) => input?.OnPointerClick(this, eventData);
		public void OnPointerEnter(PointerEventData eventData) => input?.OnPointerEnter(this, eventData);
		public void OnPointerExit(PointerEventData eventData) => input?.OnPointerExit(this, eventData);

		public void OnBeginDrag(PointerEventData eventData)
		{
			if (input != null && input.OnBeginDrag(this, eventData))
			{
				isDragging = true;
				RefreshVisuals();
			}
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			if (input != null && input.OnEndDrag(this, eventData))
			{
				isDragging = false;
				RefreshVisuals();
			}
		}

		public void OnDrag(PointerEventData eventData) => input?.OnDrag(this, eventData);
		public void OnDrop(PointerEventData eventData) => input?.OnDrop(this, eventData);
	}
}