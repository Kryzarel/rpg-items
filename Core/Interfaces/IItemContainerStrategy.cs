namespace Kryz.RPG.Items
{
	public interface IItemContainerStrategy
	{
		/// <summary>
		/// Get the maximum stack size of a particular item.
		/// </summary>
		/// <param name="item">The item to check.</param>
		/// <returns>The maximum stack size for the <paramref name="item"/>.</returns>
		int GetMaxStacks(IItem item);

		bool CanReceive(int index, in ItemStack stack);

		bool CanRemove(int index);

		/// <summary>
		/// Try to add an item stack to any inventory slots that can receive it.
		/// </summary>
		/// <param name="stack">The item stack to add.</param>
		/// <param name="remainder">If <paramref name="stack"/> could not be added in its entirety, this will contain how much remains of the original stack.</param>
		/// <returns><see cref="true"/> if the inventory had enough space for at least 1 item. Otherwise <see cref="false"/>.</returns>
		bool TryAdd(ref ItemContainerTransactionContext ctx, in ItemStack stack, out ItemStack remainder);

		/// <summary>
		/// Try to add an item stack to a specific inventory slot.
		/// </summary>
		/// <param name="index">The slot index to add to.</param>
		/// <param name="stack">The item stack to add.</param>
		/// <param name="result">The resulting item stack at <paramref name="index"/>, after adding.</param>
		/// <param name="remainder">If <paramref name="stack"/> could not be added in its entirety, this will contain how much remains of the original stack.</param>
		/// <returns><see cref="true"/> if the slot had enough space for at least 1 item. Otherwise <see cref="false"/>.</returns>
		bool TryAdd(ref ItemContainerTransactionContext ctx, int index, in ItemStack stack, out ItemStack result, out ItemStack remainder);

		/// <summary>
		/// Try to remove a certain amount of a specific item from the inventory.
		/// </summary>
		/// <param name="stack">The item stack to remove.</param>
		/// <returns><see cref="true"/> if the inventory had enough of the specified item and it was removed. Otherwise <see cref="false"/>.</returns>
		bool TryRemove(ref ItemContainerTransactionContext ctx, in ItemStack stack);

		/// <summary>
		/// Try to remove a certain amount of items from a specific inventory slot.
		/// </summary>
		/// <param name="index">The slot index to remove from.</param>
		/// <param name="count">The amount of items to remove. Passing in a negative number removes the entire stack.</param>
		/// <param name="result">The resulting item stack at <paramref name="index"/>, after adding.</param>
		/// <param name="removed">The item stack that was removed.</param>
		/// <returns><see cref="true"/> if the slot had enough of the specified item and it was removed. Otherwise <see cref="false"/>.</returns>
		bool TryRemove(ref ItemContainerTransactionContext ctx, int index, int count, out ItemStack result, out ItemStack removed);
	}
}