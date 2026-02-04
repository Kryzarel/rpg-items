namespace Kryz.RPG.Items
{
	public readonly ref struct ItemContainerTransactionContext
	{
		private readonly IItemContainer container;
		private readonly ItemContainerTransaction transaction;

		public void SetSlot(int index, ItemStack value)
		{
			transaction.SetSlot(container, index, value);
		}

		public ItemStack GetSlot(int index)
		{
			return transaction.GetSlot(container, index);
		}
	}
}