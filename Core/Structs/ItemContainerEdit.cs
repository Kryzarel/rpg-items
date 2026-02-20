using System;

namespace Kryz.RPG.Items
{
	public readonly struct ItemContainerEdit : IComparable<ItemContainerEdit>
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

		public int CompareTo(ItemContainerEdit other)
		{
			int result = Container.InstanceId.CompareTo(other.Container.InstanceId);
			if (result != 0) return result;
			result = IsSizeEdit.CompareTo(other.IsSizeEdit);
			if (result != 0) return result;
			result = SlotEdit.Index.CompareTo(other.SlotEdit.Index);
			return result;
		}
	}
}