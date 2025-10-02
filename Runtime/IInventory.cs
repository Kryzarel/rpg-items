using System.Collections.Generic;

namespace Kryz.RPG.Items
{
	public interface IInventory
	{
		int NumSlots { get; }

		IReadOnlyList<ItemStack> Slots { get; }

		bool TryAdd(ItemStack stack);
		bool TryAdd(int index, ItemStack stack);
		bool TryRemove(int index, out ItemStack stack);
		bool TryMove(int from, int to);
	}
}