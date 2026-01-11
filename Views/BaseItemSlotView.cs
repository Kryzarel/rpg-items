using UnityEngine;

namespace Kryz.RPG.Items.Views
{
	[RequireComponent(typeof(RectTransform))]
	public abstract class BaseItemSlotView : MonoBehaviour, IItemSlotView
	{
		public abstract int SlotIndex { get; }
		public abstract IInventory? Inventory { get; }
		public abstract IItemSlotInput? Input { get; }
		public abstract ItemStack ItemStack { get; }

		public RectTransform RectTransform => (RectTransform)transform;

		public abstract void Bind(int slotIndex, IInventory inventory, IItemSlotInput? input = null);
		public abstract void Unbind();
		public abstract void SetItemStack(ItemStack stack);
		public abstract void SetDragVisuals(bool isDragging);
	}
}