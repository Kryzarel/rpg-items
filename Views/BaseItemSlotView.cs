using UnityEngine;

namespace Kryz.RPG.Items.Views
{
	[RequireComponent(typeof(RectTransform))]
	public abstract class BaseItemSlotView : MonoBehaviour, IItemSlotView
	{
		public abstract int Index { get; }
		public abstract IInventory? Inventory { get; }
		public abstract IItemSlotInput? Input { get; }
		public abstract ItemStack ItemStack { get; }

		public RectTransform RectTransform => (RectTransform)transform;

		public abstract void Setup(int index, IInventory? inventory, IItemSlotInput? input);
		public abstract void SetItemStack(ItemStack stack);
	}
}