using UnityEngine;

namespace Kryz.RPG.Items.Views
{
	public abstract class BaseItemSlotView : MonoBehaviour, IItemSlotView
	{
		public abstract int Index { get; }
		public abstract IInventory? Inventory { get; }
		public abstract ItemStack ItemStack { get; }

		public abstract void Setup(int index, IInventory? inventory, IItemSlotInteraction? interaction);
		public abstract void SetItemStack(ItemStack stack);
	}
}