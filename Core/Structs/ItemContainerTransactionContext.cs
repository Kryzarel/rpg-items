namespace Kryz.RPG.Items
{
	public readonly ref struct ItemContainerTransactionContext
	{
		private readonly IItemContainer container;
		private readonly ItemContainerTransaction transaction;

		public int GetSize()
		{
			return transaction.GetSize(container);
		}

		public void SetSize(int size)
		{
			transaction.SetSize(container, size);
		}

		public ItemStack GetSlot(int index)
		{
			return transaction.GetSlot(container, index);
		}

		public void SetSlot(int index, ItemStack value)
		{
			transaction.SetSlot(container, index, value);
		}
	}
}