namespace Kryz.RPG.Items
{
	public readonly struct ItemContainerEdit
	{
		public readonly IItemContainerTransactional Container;
		public readonly SizeEdit SizeEdit;
		public readonly SlotEdit SlotEdit;
		public readonly bool IsSizeEdit;

		public ItemContainerEdit(IItemContainerTransactional container, SizeEdit sizeEdit)
		{
			Container = container;
			SizeEdit = sizeEdit;
			SlotEdit = default;
			IsSizeEdit = true;
		}

		public ItemContainerEdit(IItemContainerTransactional container, SlotEdit slotEdit)
		{
			Container = container;
			SizeEdit = default;
			SlotEdit = slotEdit;
			IsSizeEdit = false;
		}
	}
}