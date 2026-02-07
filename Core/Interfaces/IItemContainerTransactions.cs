using System;

namespace Kryz.RPG.Items
{
	public interface IItemContainerTransactions
	{
		/// <summary>
		/// Try to set the container to the specified size.
		/// </summary>
		/// <param name="value">The size to set.</param>
		/// <returns><see cref="true"/> if it was possible to resize the container. Otherwise <see cref="false"/>.</returns>
		bool TrySetSize(ItemContainerTransaction transaction, int value);

		/// <summary>
		/// Try to add an item stack to any slots that can receive it.
		/// </summary>
		/// <param name="stack">The item stack to add.</param>
		/// <param name="remainder">If <paramref name="stack"/> could not be added in its entirety, this will contain how much remains of the original stack.</param>
		/// <returns><see cref="true"/> if the container had enough space for at least 1 item. Otherwise <see cref="false"/>.</returns>
		bool TryAdd(ItemContainerTransaction transaction, in ItemStack stack, out ItemStack remainder);

		/// <summary>
		/// Try to add an item stack to a specific slot.
		/// </summary>
		/// <param name="index">The slot index to add to.</param>
		/// <param name="stack">The item stack to add.</param>
		/// <param name="result">The resulting item stack at <paramref name="index"/>, after adding.</param>
		/// <param name="remainder">If <paramref name="stack"/> could not be added in its entirety, this will contain how much remains of the original stack.</param>
		/// <returns><see cref="true"/> if the slot had enough space for at least 1 item. Otherwise <see cref="false"/>.</returns>
		bool TryAdd(ItemContainerTransaction transaction, int index, in ItemStack stack, out ItemStack result, out ItemStack remainder);

		/// <summary>
		/// Try to remove a certain amount of a specific item from the container.
		/// </summary>
		/// <param name="stack">The item stack to remove.</param>
		/// <returns><see cref="true"/> if the container had enough of the specified item and it was removed. Otherwise <see cref="false"/>.</returns>
		bool TryRemove(ItemContainerTransaction transaction, in ItemStack stack);

		/// <summary>
		/// Try to remove a certain amount of items from a specific slot.
		/// </summary>
		/// <param name="index">The slot index to remove from.</param>
		/// <param name="count">The amount of items to remove. Passing in a negative number removes the entire stack.</param>
		/// <param name="result">The resulting item stack at <paramref name="index"/>, after adding.</param>
		/// <param name="removed">The item stack that was removed.</param>
		/// <returns><see cref="true"/> if the slot had enough of the specified item and it was removed. Otherwise <see cref="false"/>.</returns>
		bool TryRemove(ItemContainerTransaction transaction, int index, int count, out ItemStack result, out ItemStack removed);

		/// <summary>
		/// Commit changes made from a transaction.
		/// </summary>
		/// <param name="slotEdits">The changes to make to the item slots.</param>
		/// <param name="sizeEdit">The change to make to the size of the container.</param>
		void Commit(ReadOnlySpan<SlotEdit> slotEdits, int? sizeEdit);
	}
}