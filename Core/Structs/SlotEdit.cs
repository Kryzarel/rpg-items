namespace Kryz.RPG.Items
{
	public readonly struct SlotEdit
	{
		public readonly int Index;
		public readonly ItemStack OldStack;
		public readonly ItemStack NewStack;

		public SlotEdit(int index, ItemStack oldStack, ItemStack newStack)
		{
			Index = index;
			OldStack = oldStack;
			NewStack = newStack;
		}
	}
}