using System;
using System.Collections.Generic;

namespace Kryz.RPG.Items
{
	public interface IReadOnlyItemContainer : IReadOnlyList<ItemStack>
	{
		ReadOnlySpan<ItemStack> Slots { get; }

		int Size { get; }
		int FreeSlots { get; }
		int OccupiedSlots { get; }
	}
}