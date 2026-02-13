namespace Kryz.RPG.Items
{
	public interface IItemTransaction
	{
		void RecordSize(IItemContainerTransactional container, int size);
		void RecordSlot(IItemContainerTransactional container, int index, ItemStack oldStack, ItemStack newStack);
	}
}