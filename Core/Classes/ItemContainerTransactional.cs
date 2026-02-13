using System.Buffers;

namespace Kryz.RPG.Items
{
	public sealed class ItemContainerTransactional : ItemContainerTransactionalBase
	{
		public ItemContainerTransactional(int size, ArrayPool<ItemStack>? pool = null) : base(size, pool)
		{
		}
	}
}