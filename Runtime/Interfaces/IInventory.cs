using System;
using System.Collections.Generic;
using Kryz.RPG.Items.Structs;

namespace Kryz.RPG.Items.Interfaces
{
	public interface IInventory : IReadOnlyList<ItemStack>
	{
		event Action<int> OnSlotChanged;
		event Action<int> OnNumSlotsChanged;

		int NumSlots { get; set; }
		int FreeSlots { get; }
		int FilledSlots { get; }

		/// <summary>
		/// <para>Get the maximum stack size for a particular item.</para>
		/// <para>Even though <see cref="IItemDefinition"/> already specifies <see cref="IItemDefinition.MaxStacks"/>, an inventory may have a special rules that allow larger stacks of certain items.</para>
		/// </summary>
		int GetMaxStackSize(IItem item);

		/// <summary>
		/// Set a slot to a specific <see cref="ItemStack"/>.
		/// </summary>
		/// <param name="index">The index of the slot to set.</param>
		/// <param name="stack">The stack to set the slot to.</param>
		void SetSlot(int index, in ItemStack stack);

		/// <summary>
		/// Check if it's possible to add an item stack to the inventory.
		/// </summary>
		/// <param name="stack">The item stack to add.</param>
		/// <param name="remainder">If <paramref name="stack"/> cannot be added in its entirety, this will contain how will be leftover of the original stack.</param>
		/// <returns>True if the inventory has enough space to add the item. False otherwise.</returns>
		bool CanAdd(in ItemStack stack, out ItemStack remainder);

		/// <summary>
		/// Try to add an item stack to any inventory slots that can receive it.
		/// </summary>
		/// <param name="stack">The item stack to add.</param>
		/// <param name="remainder">If <paramref name="stack"/> could not be added in its entirety, this will contain how much remains of the original stack.</param>
		/// <returns>True if the inventory had enough space and the item was added. False otherwise.</returns>
		bool TryAdd(in ItemStack stack, out ItemStack remainder);

		/// <summary>
		/// Try to add an item stack to a specific inventory slot.
		/// </summary>
		/// <param name="index">The slot index to add to.</param>
		/// <param name="stack">The item stack to add.</param>
		/// <param name="remainder">If <paramref name="stack"/> could not be added in its entirety, this will contain how much remains of the original stack.</param>
		/// <returns>True if the slot had enough space and the item was added. False otherwise.</returns>
		bool TryAdd(int index, in ItemStack stack, out ItemStack remainder);

		/// <summary>
		/// Try to remove a certain amount of a specific item from the inventory.
		/// </summary>
		/// <param name="item">The item to remove.</param>
		/// <param name="count">The amount to remove.</param>
		/// <param name="removed">The item stack that was removed.</param>
		/// <returns>True if the inventory had enough of the specified item and it was removed. False otherwise.</returns>
		bool TryRemove(IItem item, int count, out ItemStack removed);

		/// <summary>
		/// Try to remove a certain amount of items from a specific inventory slot.
		/// </summary>
		/// <param name="index">The slot index to remove from.</param>
		/// <param name="count">The amount of items to remove.</param>
		/// <param name="removed">The item stack that was removed.</param>
		/// <returns>True if the slot had a valid item stack and it was removed. False otherwise.</returns>
		bool TryRemove(int index, int count, out ItemStack removed);

		/// <summary>
		/// Try to remove an entire item stack from a specific inventory slot.
		/// </summary>
		/// <param name="index">The slot index to remove from.</param>
		/// <param name="removed">The item stack that was removed.</param>
		/// <returns>True if the slot had a valid item stack and it was removed. False otherwise.</returns>
		bool TryRemove(int index, out ItemStack removed);

		/// <summary>
		/// <para>Try to move an item stack from a specific inventory slot to another.</para>
		/// <para>If the '<paramref name="toIndex"/>' slot can accept the item stack (same item and enough space), it is simply moved.</para>
		/// <para>If '<paramref name="toIndex"/>' can't accept the item AND '<paramref name="allowSwap"/>' is true, the '<paramref name="toIndex"/>' and '<paramref name="fromIndex"/>' item stacks are swapped. Otherwise nothing happens.</para>
		/// </summary>
		/// <param name="toIndex">The slot index to move to.</param>
		/// <param name="fromIndex">The slot index to move from.</param>
		/// <param name="fromInventory">The inventory to move from.</param>
		/// <param name="allowSwap">If we should swap the stacks when the target inventory doesn't have enough space.</param>
		/// <returns>True if '<paramref name="fromIndex"/>' has a valid item stack and it was successfully moved to or swapped with '<paramref name="toIndex"/>'. False otherwise.</returns>
		bool TryMove(int toIndex, int fromIndex, IInventory fromInventory, bool allowSwap = false);
	}
}