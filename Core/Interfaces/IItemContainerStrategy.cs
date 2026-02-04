using System;

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

		/// <summary>
		/// Try to add an item stack to any inventory slots that can receive it.
		/// </summary>
		/// <param name="stack">The item stack to add.</param>
		/// <param name="remainder">If <paramref name="stack"/> could not be added in its entirety, this will contain how much remains of the original stack.</param>
		/// <returns>True if the inventory had enough space for at least 1 item. False otherwise.</returns>
		bool TryAdd(Span<ItemStack> slots, in ItemStack stack, out ItemStack remainder);

		/// <summary>
		/// Try to add an item stack to a specific inventory slot.
		/// </summary>
		/// <param name="stack">The item stack to add.</param>
		/// <param name="remainder">If <paramref name="stack"/> could not be added in its entirety, this will contain how much remains of the original stack.</param>
		/// <returns>True if the slot had enough space for at least 1 item. False otherwise.</returns>
		bool TryAdd(Span<ItemStack> slots, int index, in ItemStack stack, out ItemStack remainder);

		/// <summary>
		/// Try to add an item stack to a specific inventory slot.
		/// </summary>
		/// <param name="stack">The item stack to add.</param>
		/// <param name="remainder">If <paramref name="stack"/> could not be added in its entirety, this will contain how much remains of the original stack.</param>
		/// <param name="maxCount">The maximum amount of items the stack can hold.</param>
		/// <returns>True if the slot had enough space for at least 1 item. False otherwise.</returns>
		bool TryAdd(Span<ItemStack> slots, int index, in ItemStack stack, out ItemStack remainder, int maxCount);

		/// <summary>
		/// Try to remove a certain amount of a specific item from the inventory.
		/// </summary>
		/// <param name="stack">The item stack to remove.</param>
		/// <returns>True if the inventory had enough of the specified item and it was removed. False otherwise.</returns>
		bool TryRemove(Span<ItemStack> slots, in ItemStack stack);

		/// <summary>
		/// Try to remove an entire item stack from a specific inventory slot.
		/// </summary>
		/// <param name="removed">The item stack that was removed.</param>
		/// <returns>True if the slot had a non-empty item stack and it was removed. False otherwise.</returns>
		bool TryRemove(Span<ItemStack> slots, int index, out ItemStack removed);

		/// <summary>
		/// Try to remove a certain amount of items from a specific inventory slot.
		/// </summary>
		/// <param name="count">The amount of items to remove.</param>
		/// <param name="removed">The item stack that was removed.</param>
		/// <returns>True if the slot had an item stack with enough items and it was removed. False otherwise.</returns>
		bool TryRemove(Span<ItemStack> slots, int index, int count, out ItemStack removed);

		/// <summary>
		/// Try to set an item slot to the specified <see cref="ItemStack"/>.
		/// </summary>
		/// <param name="stack">The <see cref="ItemStack"/> to assign to the slot.</param>
		/// <param name="previous">The <see cref="ItemStack"/> that was previously in the slot.</param>
		/// <returns>True if the inventory slot was set to the specified <see cref="ItemStack"/>. False otherwise.</returns>
		bool TrySet(Span<ItemStack> slots, int index, in ItemStack stack, out ItemStack previous);
	}
}