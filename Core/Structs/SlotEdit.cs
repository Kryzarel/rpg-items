namespace Kryz.RPG.Items
{
	public readonly struct SlotEdit
	{
		public readonly int Index;
		public readonly ItemStack NewStack;

		public SlotEdit(int index, ItemStack newStack)
		{
			Index = index;
			NewStack = newStack;
		}
	}
}