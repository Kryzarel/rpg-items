using Kryz.RPG.Items.Structs;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kryz.RPG.Items.UI
{
	public class ItemSlotView : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler
	{
		[SerializeField] Image image;
		[SerializeField] Text countText;

		private ItemStack itemStack;

		public ItemStack ItemStack => itemStack;

		public int Index;
		public IItemSlotInteraction? Interaction;

		private void Awake()
		{
			image.sprite = null!;
			countText.text = string.Empty;
		}

		public void SetItemStack(ItemStack stack)
		{
			if (itemStack == stack)
			{
				return;
			}

			if (image.sprite != stack.Item?.Definition.Icon)
			{
				image.sprite = stack.Item?.Definition.Icon!;
			}

			if (itemStack.Count != stack.Count)
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