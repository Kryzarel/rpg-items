using System;
using System.Collections.Generic;

namespace Kryz.RPG.Items
{
	public interface IItemContainerTransactional : IReadOnlyItemContainer
	{
		public delegate void OnSlotChangedDelegate(int index, ItemStack oldStack, ItemStack newStack);
		public delegate void OnSizeChangedDelegate(int oldSize, int newSize);

		event OnSlotChangedDelegate OnSlotChanged;
		event OnSizeChangedDelegate OnSizeChanged;
		event Action OnTransactionComplete;

		/// <summary>
		/// Try to add an <see cref="ItemStack"/> to any slots that can receive it.
		/// </summary>
		/// <param name="stack">The <see cref="ItemStack"/> to add.</param>
		/// <param name="remainder">If <paramref name="stack"/> could not be added in its entirety, this will contain how much remains of the original stack.</param>
		/// <returns><see cref="true"/> if the container had enough space for at least 1 item. Otherwise <see cref="false"/>.</returns>
		bool TryAdd<T>(T transaction, in ItemStack stack, out ItemStack remainder) where T : IItemTransaction;

		/// <summary>
		/// Try to add an <see cref="ItemStack"/> to a specific slot.
		/// </summary>
		/// <param name="index">The slot index to add to.</param>
		/// <param name="stack">The <see cref="ItemStack"/> to add.</param>
		/// <param name="result">The resulting <see cref="ItemStack"/> at <paramref name="index"/>, after adding.</param>
		/// <param name="remainder">If <paramref name="stack"/> could not be added in its entirety, this will contain how much remains of the original stack.</param>
		/// <returns><see cref="true"/> if the slot had enough space for at least 1 item. Otherwise <see cref="false"/>.</returns>
		bool TryAddSlot<T>(T transaction, int index, in ItemStack stack, out ItemStack result, out ItemStack remainder) where T : IItemTransaction;

		/// <summary>
		/// Try to remove a certain amount of a specific item from the container.
		/// </summary>
		/// <param name="stack">The <see cref="ItemStack"/> to remove.</param>
		/// <returns><see cref="true"/> if the container had enough of the specified item and it was removed. Otherwise <see cref="false"/>.</returns>
		bool TryRemove<T>(T transaction, in ItemStack stack) where T : IItemTransaction;

		/// <summary>
		/// Try to remove a certain amount of items from a specific slot.
		/// </summary>
		/// <param name="index">The slot index to remove from.</param>
		/// <param name="result">The resulting <see cref="ItemStack"/> at <paramref name="index"/>, after adding.</param>
		/// <param name="removed">The <see cref="ItemStack"/> that was removed.</param>
		/// <param name="count">The amount of items to remove. Passing in a negative number removes the entire stack.</param>
		/// <returns><see cref="true"/> if the slot had enough of the specified item and it was removed. Otherwise <see cref="false"/>.</returns>
		bool TryRemoveSlot<T>(T transaction, int index, out ItemStack result, out ItemStack removed, int count = -1) where T : IItemTransaction;

		/// <summary>
		/// Try to move a certain amount of items from a specific slot to another <see cref="IItemContainer"/>.
		/// </summary>
		/// <param name="fromIndex">The slot index to move from.</param>
		/// <param name="to">The <see cref="IItemContainer"/> to move to.</param>
		/// <param name="count">The amount of items to move. Passing in a negative number moves the entire stack.</param>
		/// <returns><see cref="true"/> if it was possible to move the specified amount of items. Otherwise <see cref="false"/>.</returns>
		public bool TryMove<T>(T transaction, int fromIndex, IItemContainerTransactional to, int count = -1) where T : IItemTransaction;

		/// <summary>
		/// Try to move a certain amount of items from a specific slot to another slot of an <see cref="IItemContainer"/>.
		/// </summary>
		/// <param name="fromIndex">The slot index to move from.</param>
		/// <param name="to">The <see cref="IItemContainer"/> to move to.</param>
		/// <param name="toIndex">The slot index of the target <see cref="IItemContainer"/> to move to.</param>
		/// <param name="count">The amount of items to move. Passing in a negative number moves the entire stack.</param>
		/// <returns><see cref="true"/> if it was possible to move the specified amount of items. Otherwise <see cref="false"/>.</returns>
		public bool TryMoveSlot<T>(T transaction, int fromIndex, IItemContainerTransactional to, int toIndex, int count = -1) where T : IItemTransaction;

		/// <summary>
		/// Try to set the container to the specified size.
		/// </summary>
		/// <param name="value">The size to set.</param>
		/// <returns><see cref="true"/> if it was possible to resize the container. Otherwise <see cref="false"/>.</returns>
		bool TrySetSize<T>(T transaction, int value) where T : IItemTransaction;

		/// <summary>
		/// Try to set a slot to the specified <see cref="ItemStack"/>.
		/// </summary>
		/// <param name="index">The slot index to set.</param>
		/// <param name="stack">The stack to set the slot to.</param>
		/// <returns><see cref="true"/> if it was possible to set the slot. Otherwise <see cref="false"/>.</returns>
		bool TrySetSlot<T>(T transaction, int index, in ItemStack stack) where T : IItemTransaction;

		/// <summary>
		/// Set a slot to the specified <see cref="ItemStack"/>. Skips most checks other methods do.
		/// </summary>
		/// <param name="index">The slot index to set.</param>
		/// <param name="stack">The stack to set the slot to.</param>
		/// <returns><see cref="true"/> if it was possible to set the slot. Otherwise <see cref="false"/>.</returns>
		void SetSlot<T>(T transaction, int index, in ItemStack stack) where T : IItemTransaction;

		void Sort<T>(T transaction) where T : IItemTransaction;

		void Sort<T, TComparer>(T transaction, TComparer comparer) where TComparer : IComparer<ItemStack> where T : IItemTransaction;

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