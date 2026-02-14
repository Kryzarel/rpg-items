namespace Kryz.RPG.Items
{
	public interface IItemTransaction
	{
		void RecordSize(IItemContainerTransactional container, int oldSize, int newSize);
		void RecordSlot(IItemContainerTransactional container, int index, ItemStack oldStack, ItemStack newStack);
	}
}