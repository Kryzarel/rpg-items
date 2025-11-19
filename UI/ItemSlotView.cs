using System;
using Kryz.RPG.Items.Structs;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kryz.RPG.Items.UI
{
	public class ItemSlotView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler
	{
		[SerializeField] Image image;
		[SerializeField] Text countText;

		private ItemStack itemStack;

		public ItemStack ItemStack => itemStack;
		public int Index;

		public event Action<ItemSlotView, PointerEventData>? OnPointerEnterEvent;
		public event Action<ItemSlotView, PointerEventData>? OnPointerExitEvent;
		public event Action<ItemSlotView, PointerEventData>? OnPointerClickEvent;
		public event Action<ItemSlotView, PointerEventData>? OnBeginDragEvent;
		public event Action<ItemSlotView, PointerEventData>? OnEndDragEvent;
		public event Action<ItemSlotView, PointerEventData>? OnDragEvent;
		public event Action<ItemSlotView, PointerEventData>? OnDropEvent;

		private void Awake() => SetItemStack(default);

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
				if (stack.Count > 1)
				{
					countText.text = stack.Count.ToString();
				}

				bool enabled = stack.Count > 0;
				if (countText.enabled != enabled)
				{
					countText.enabled = enabled;
				}
			}

			itemStack = stack;
		}

		public void OnPointerEnter(PointerEventData eventData) => OnPointerEnterEvent?.Invoke(this, eventData);
		public void OnPointerExit(PointerEventData eventData) => OnPointerExitEvent?.Invoke(this, eventData);
		public void OnPointerClick(PointerEventData eventData) => OnPointerClickEvent?.Invoke(this, eventData);
		public void OnBeginDrag(PointerEventData eventData) => OnBeginDragEvent?.Invoke(this, eventData);
		public void OnEndDrag(PointerEventData eventData) => OnEndDragEvent?.Invoke(this, eventData);
		public void OnDrag(PointerEventData eventData) => OnDragEvent?.Invoke(this, eventData);
		public void OnDrop(PointerEventData eventData) => OnDropEvent?.Invoke(this, eventData);
	}
}