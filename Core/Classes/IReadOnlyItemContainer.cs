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

		/// <summary>
		/// Get the maximum stack size of a particular item.
		/// </summary>
		/// <param name="item">The item to check.</param>
		/// <returns>The maximum stack size for the <paramref name="item"/>.</returns>
		int GetMaxStacks(IItem item);

		/// <summary>
		/// Check if the slot at <paramref name="index"/> can receive the specified <see cref="ItemStack"/>.
		/// </summary>
		/// <param name="index">The slot index to check.</param>
		/// <param name="stack">The <see cref="ItemStack"/> to check.</param>
		/// <returns><see cref="true"/> if the slot can receive the <paramref name="stack"/>. Otherwise <see cref="false"/>.</returns>
		bool CanReceive(int index, in ItemStack stack);

		/// <summary>
		/// Check if the slot at <paramref name="index"/> can remove any of its contents.
		/// </summary>
		/// <param name="index">The slot index to check.</param>
		/// <returns><see cref="true"/> items can be removed from the slot. Otherwise <see cref="false"/>.</returns>
		bool CanRemove(int index);

		/// <summary>
		/// Check if it's possible to add an <see cref="ItemStack"/> to the container.
		/// </summary>
		/// <param name="stack">The <see cref="ItemStack"/> to add.</param>
		/// <param name="remainder">If <paramref name="stack"/> cannot be added in its entirety, this will contain how much remains of the original stack.</param>
		/// <returns><see cref="true"/> if at least 1 item can be added. Otherwise <see cref="false"/>.</returns>
		bool CanAdd(in ItemStack stack, out ItemStack remainder);

		/// <summary>
		/// Check if it's possible to add an <see cref="ItemStack"/> to a specific slot.
		/// </summary>
		/// <param name="index">The slot index to add to.</param>
		/// <param name="stack">The <see cref="ItemStack"/> to add.</param>
		/// <param name="result">The resulting <see cref="ItemStack"/> at <paramref name="index"/>, after adding.</param>
		/// <param name="remainder">If <paramref name="stack"/> cannot be added in its entirety, this will contain how much remains of the original stack.</param>
		/// <returns><see cref="true"/> if at least 1 item can be added. Otherwise <see cref="false"/>.</returns>
		bool CanAddSlot(int index, in ItemStack stack, out ItemStack result, out ItemStack remainder);

		/// <summary>
		/// Check if it's possible to remove a certain amount of a specific item from the container.
		/// </summary>
		/// <param name="stack">The <see cref="ItemStack"/> to remove.</param>
		/// <returns><see cref="true"/> if the container has enough of the specified item and it can be removed. Otherwise <see cref="false"/>.</returns>
		bool CanRemove(in ItemStack stack);

		/// <summary>
		/// Check if it's possible to remove a certain amount of items from a specific slot.
		/// </summary>
		/// <param name="index">The slot index to remove from.</param>
		/// <param name="result">The resulting <see cref="ItemStack"/> at <paramref name="index"/>, after removing.</param>
		/// <param name="removed">The <see cref="ItemStack"/> that would be removed.</param>
		/// <param name="count">The amount of items to remove. Passing in a negative number removes the entire stack.</param>
		/// <returns><see cref="true"/> if the slot had enough of the specified item and it was removed. Otherwise <see cref="false"/>.</returns>
		bool CanRemoveSlot(int index, out ItemStack result, out ItemStack removed, int count = -1);

		/// <summary>
		/// Check if it's possible to set the size of the container.
		/// </summary>
		/// <param name="value">The size to set.</param>
		/// <returns><see cref="true"/> if the size can be set. Otherwise <see cref="false"/>.</returns>
		bool CanSetSize(int value);
	}
}