using Kryz.RPG.Items.Interfaces;
using Kryz.RPG.Items.Structs;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kryz.RPG.Items.UI
{
	public class ItemSlotView : MonoBehaviour, IItemSlotView, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler
	{
		[SerializeField] Image image;
		[SerializeField] Text countText;

		public int Index { get; private set; }
		public IInventory? Inventory { get; private set; }
		public IItemSlotInteraction? Interaction { get; private set; }
		public ItemStack ItemStack { get; private set; }

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
			image.sprite = null!;
			countText.text = string.Empty;
		}

		public void Setup(int index, IInventory? inventory, IItemSlotInteraction? interaction)
		{
			Index = index;
			Inventory = inventory;
			Interaction = interaction;
		}

		public void SetItemStack(ItemStack stack)
		{
			if (ItemStack == stack)
			{
				return;
			}

			if (image.sprite != stack.Item?.Icon)
			{
				image.sprite = stack.Item?.Icon!;
			}

			if (ItemStack.Count != stack.Count)
			{
				countText.text = stack.Count > 1 ? stack.Count.ToString() : string.Empty;
			}

			ItemStack = stack;
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