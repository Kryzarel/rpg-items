namespace Kryz.RPG.Items
{
	public readonly struct SlotEdit
	{
		public readonly IItemContainer Container;
		public readonly int Index;
		public readonly ItemStack OldStack;
		public readonly ItemStack NewStack;

		public SlotEdit(IItemContainer container, int index, ItemStack oldStack, ItemStack newStack)
		{
			Container = container;
			Index = index;
			OldStack = oldStack;
			NewStack = newStack;
		}
	}
}