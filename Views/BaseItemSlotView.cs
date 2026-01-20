using UnityEngine;

namespace Kryz.RPG.Items.Views
{
	[RequireComponent(typeof(RectTransform))]
	public abstract class BaseItemSlotView : MonoBehaviour, IItemSlotView
	{
		public abstract int SlotIndex { get; }
		public abstract IInventory? Inventory { get; }
		public abstract IItemSlotController? Controller { get; }
		public abstract ItemStack ItemStack { get; }

		public RectTransform RectTransform => (RectTransform)transform;

		public abstract void Bind(int slotIndex, IInventory inventory, IItemSlotController? controller = null);
		public abstract void Unbind();
		public abstract void SetItemStack(ItemStack stack);
	}
}