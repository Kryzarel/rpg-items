using System.Collections.Generic;
using Kryz.RPG.Items.Structs;

namespace Kryz.RPG.Items.Interfaces
{
	public interface IInventory : IReadOnlyList<ItemStack>
	{
		int NumSlots { get; set; }

		/// <summary>
		/// Try to add an item stack to the first inventory slot that can receive it.
		/// </summary>
		/// <param name="stack">The item stack to add.</param>
		/// <returns>True if the inventory had enough space and the item was added. False otherwise.</returns>
		bool TryAdd(ItemStack stack, out ItemStack remainder);

		/// <summary>
		/// Try to add an item stack to a specific inventory slot.
		/// </summary>
		/// <param name="index">The slot index to add to.</param>
		/// <param name="stack">The item stack to add.</param>
		/// <returns>True if the slot had enough space and the item was added. False otherwise.</returns>
		bool TryAdd(int index, ItemStack stack, out ItemStack remainder);

		/// <summary>
		/// Try to remove an item stack from a specific inventory slot.
		/// </summary>
		/// <param name="index">The slot index to remove from.</param>
		/// <param name="stack">The item stack that was removed.</param>
		/// <returns>True if the slot had a valid item stack and it was removed. False otherwise.</returns>
		bool TryRemove(int index, out ItemStack stack);

		/// <summary>
		/// <para>Try to move an item stack from a specific inventory slot to another.</para>
		/// <para>If the '<paramref name="to"/>' slot has enough space to the accept the item stack, it is simply moved.</para>
		/// <para>Otherwise, the '<paramref name="to"/>' and '<paramref name="from"/>' item stacks are swapped.</para>
		/// </summary>
		/// <param name="from">The slot index to move from.</param>
		/// <param name="to">The slot index to move to.</param>
		/// <returns>True if '<paramref name="from"/>' has a valid item stack and it was successfully moved to or swapped with '<paramref name="to"/>'. False otherwise.</returns>
		bool TryMove(int from, int to);
	}
}