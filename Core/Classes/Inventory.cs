using System.Buffers;

namespace Kryz.RPG.Items
{
	public sealed class Inventory : ItemContainer
	{
		public Inventory(int size, ArrayPool<ItemStack>? pool = null) : base(size, pool)
		{
		}
	}
}