using Kryz.RPG.Items.Interfaces;
using Kryz.RPG.Items.Structs;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kryz.RPG.Items.Views
{
	public class ItemSlotView : MonoBehaviour, IItemSlotView, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler
	{
		[SerializeField] Image? image;
		[SerializeField] Text? countText;

		private int index;
		private IInventory? inventory;
		private IItemSlotInteraction? interaction;
		private ItemStack itemStack;

		public int Index => index;
		public IInventory? Inventory => inventory;
		public IItemSlotInteraction? Interaction => interaction;
		public ItemStack ItemStack => itemStack;

		private void OnValidate()
		{
			if (image == null) image = GetComponentInChildren<Image>();
			if (countText == null) countText = GetComponentInChildren<Text>();

			if (image != null) image.raycastTarget = true;
			if (countText != null) countText.raycastTarget = false;
		}

		private void Awake()
		{
			OnValidate();
			image!.sprite = null!;
			countText!.text = string.Empty;
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

			if (image != null && image.sprite != stack.Item?.Icon)
			{
				image.sprite = stack.Item?.Icon!;
			}

			if (countText != null && itemStack.Count != stack.Count)
			{
				countText.text = stack.Count > 1 ? stack.Count.ToString() : string.Empty;
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