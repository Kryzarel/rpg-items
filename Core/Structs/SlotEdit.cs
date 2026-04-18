namespace Kryz.RPG.Items
{
	public readonly struct SlotEdit
	{
		public readonly int Index;
		public readonly ItemStack OldStack;
		public readonly ItemStack NewStack;

		public bool IsEmpty => Index == 0 && OldStack == default && NewStack == default;

		public SlotEdit(int index, ItemStack oldStack, ItemStack newStack)
		{
			Index = index;
			OldStack = oldStack;
			NewStack = newStack;
		}
	}
}