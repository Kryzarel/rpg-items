using Kryz.RPG.Items.Interfaces;
using Kryz.RPG.Items.Structs;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kryz.RPG.Items.Views
{
	public class ItemSlotView : MonoBehaviour, IItemSlotView, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler
	{
		[Header("References")]
		[SerializeField] Image? raycastImage;
		[SerializeField] Image? itemImage;
		[SerializeField] TextMeshProUGUI? text;
		[Header("Settings")]
		[SerializeField] string textFormatString = string.Empty;

		private int index;
		private IInventory? inventory;
		private IItemSlotInteraction? interaction;
		private ItemStack itemStack;

		public Image RaycastImage => raycastImage!;
		public Image ItemImage => itemImage!;
		public TextMeshProUGUI Text => text!;

		public int Index => index;
		public IInventory? Inventory => inventory;
		public IItemSlotInteraction? Interaction => interaction;
		public ItemStack ItemStack => itemStack;

		private void OnValidate()
		{
			if (raycastImage == null || itemImage == null)
			{
				Image[] images = GetComponentsInChildren<Image>(includeInactive: true);
				if (images.Length > 0) raycastImage = images[0];
				if (images.Length > 1) itemImage = images[1];
			}

			if (text == null) text = GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);

			if (raycastImage != null) raycastImage.raycastTarget = false;
			if (itemImage != null) itemImage.raycastTarget = false;
			if (text != null) text.raycastTarget = false;
		}

		private void Awake()
		{
			OnValidate();
			raycastImage!.sprite = null!;
			itemImage!.sprite = null!;
			text!.text = string.Empty;
		}

		public void Setup(int index, IInventory? inventory, IItemSlotInteraction? interaction)
		{
			this.index = index;
			this.inventory = inventory;
			this.interaction = interaction;
		}

		public void SetItemStack(ItemStack stack)
		{
			if (itemStack == stack)
			{
				return;
			}

			if (itemImage != null && itemImage.sprite != stack.Item?.Icon)
			{
				itemImage.sprite = stack.Item?.Icon!;
				itemImage.enabled = stack.Item != null;
			}

			if (text != null && itemStack.Count != stack.Count)
			{
				bool enabled = stack.Count > 1;
				if (enabled)
				{
					text.text = string.Format(textFormatString, stack.Count);
				}
				text.enabled = enabled;
			}

			itemStack = stack;
		}

		public void OnPointerClick(PointerEventData eventData) => Interaction?.OnPointerClick(this, eventData);
		public void OnPointerEnter(PointerEventData eventData) => Interaction?.OnPointerEnter(this, eventData);
		public void OnPointerExit(PointerEventData eventData) => Interaction?.OnPointerExit(this, eventData);
		public void OnBeginDrag(PointerEventData eventData) => Interaction?.OnBeginDrag(this, eventData);
		public void OnEndDrag(PointerEventData eventData) => Interaction?.OnEndDrag(this, eventData);
		public void OnDrag(PointerEventData eventData) => Interaction?.OnDrag(this, eventData);
		public void OnDrop(PointerEventData eventData) => Interaction?.OnDrop(this, eventData);
	}
}