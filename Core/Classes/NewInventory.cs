using System.Buffers;

namespace Kryz.RPG.Items
{
	public sealed class NewInventory : ItemContainerBase
	{
		public NewInventory(int size, ArrayPool<ItemStack>? pool = null) : base(size, pool)
		{
		}
	}
}