using System.Buffers;

namespace Kryz.RPG.Items
{
	public sealed class StandardTransactions : ItemContainerTransactional
	{
		public StandardTransactions(int size, ArrayPool<ItemStack>? pool = null) : base(size, pool)
		{
		}
	}
}