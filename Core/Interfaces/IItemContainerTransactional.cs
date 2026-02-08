using System;

namespace Kryz.RPG.Items
{
	public interface IItemContainerTransactional
	{
		public delegate void OnSlotChangedDelegate(int index, ItemStack oldStack, ItemStack newStack);
		public delegate void OnSizeChangedDelegate(int oldSize, int newSize);

		event OnSlotChangedDelegate OnSlotChanged;
		event OnSizeChangedDelegate OnSizeChanged;
		event Action OnChangesFinished;

		ItemStack this[int index] { get; }

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
		/// Try to add an item stack to any slots that can receive it.
		/// </summary>
		/// <param name="stack">The item stack to add.</param>
		/// <param name="remainder">If <paramref name="stack"/> could not be added in its entirety, this will contain how much remains of the original stack.</param>
		/// <returns><see cref="true"/> if the container had enough space for at least 1 item. Otherwise <see cref="false"/>.</returns>
		bool TryAdd(ItemTransaction transaction, in ItemStack stack, out ItemStack remainder);

		/// <summary>
		/// Try to add an item stack to a specific slot.
		/// </summary>
		/// <param name="index">The slot index to add to.</param>
		/// <param name="stack">The item stack to add.</param>
		/// <param name="result">The resulting item stack at <paramref name="index"/>, after adding.</param>
		/// <param name="remainder">If <paramref name="stack"/> could not be added in its entirety, this will contain how much remains of the original stack.</param>
		/// <returns><see cref="true"/> if the slot had enough space for at least 1 item. Otherwise <see cref="false"/>.</returns>
		bool TryAdd(ItemTransaction transaction, int index, in ItemStack stack, out ItemStack result, out ItemStack remainder);

		/// <summary>
		/// Try to remove a certain amount of a specific item from the container.
		/// </summary>
		/// <param name="stack">The item stack to remove.</param>
		/// <returns><see cref="true"/> if the container had enough of the specified item and it was removed. Otherwise <see cref="false"/>.</returns>
		bool TryRemove(ItemTransaction transaction, in ItemStack stack);

		/// <summary>
		/// Try to remove a certain amount of items from a specific slot.
		/// </summary>
		/// <param name="index">The slot index to remove from.</param>
		/// <param name="count">The amount of items to remove. Passing in a negative number removes the entire stack.</param>
		/// <param name="result">The resulting item stack at <paramref name="index"/>, after adding.</param>
		/// <param name="removed">The item stack that was removed.</param>
		/// <returns><see cref="true"/> if the slot had enough of the specified item and it was removed. Otherwise <see cref="false"/>.</returns>
		bool TryRemove(ItemTransaction transaction, int index, int count, out ItemStack result, out ItemStack removed);

		/// <summary>
		/// Try to set the container to the specified size.
		/// </summary>
		/// <param name="value">The size to set.</param>
		/// <returns><see cref="true"/> if it was possible to resize the container. Otherwise <see cref="false"/>.</returns>
		bool TrySetSize(ItemTransaction transaction, int value);

		/// <summary>
		/// Try to set a slot to the specified item stack.
		/// </summary>
		/// <param name="index">The slot index to set.</param>
		/// <param name="stack">The stack to set the slot to.</param>
		/// <returns><see cref="true"/> if it was possible to set the slot. Otherwise <see cref="false"/>.</returns>
		bool TrySetSlot(ItemTransaction transaction, int index, in ItemStack stack);

		/// <summary>
		/// Notify observers of changes made after a successful transaction.
		/// </summary>
		/// <param name="slotEdits">The slots that were edited by the transaction.</param>
		/// <param name="sizeEdit">The size of the container, if it was edited by the transaction.</param>
		void Notify(ReadOnlySpan<SlotEdit> slotEdits, SizeEdit? sizeEdit);

		/// <summary>
		/// Rollback changes made after a failed transaction.
		/// </summary>
		/// <param name="slotEdits">The slots that were edited by the transaction.</param>
		/// <param name="sizeEdit">The size of the container, if it was edited by the transaction.</param>
		void Rollback(ReadOnlySpan<SlotEdit> slotEdits, SizeEdit? sizeEdit);
	}
}