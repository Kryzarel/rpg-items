using System;
using System.Collections.Generic;

namespace Kryz.RPG.Items
{
	public interface IItemContainer : IReadOnlyList<ItemStack>
	{
		IItemContainerTransactional Transactions { get; }

		ReadOnlySpan<ItemStack> Slots { get; }

		int Size { get; }
		int FreeSlots { get; }
		int OccupiedSlots { get; }

		/// <summary>
		/// Check if it's possible to add an <see cref="ItemStack"/> to the container.
		/// </summary>
		/// <param name="stack">The <see cref="ItemStack"/> to add.</param>
		/// <param name="remainder">If <paramref name="stack"/> cannot be added in its entirety, this will contain how much remains of the original stack.</param>
		/// <returns><see cref="true"/> if at least 1 item can be added. Otherwise <see cref="false"/>.</returns>
		bool CanAdd(in ItemStack stack, out ItemStack remainder);

		/// <summary>
		/// Try to add an <see cref="ItemStack"/> to any slots that can receive it.
		/// </summary>
		/// <param name="stack">The <see cref="ItemStack"/> to add.</param>
		/// <param name="remainder">If <paramref name="stack"/> could not be added in its entirety, this will contain how much remains of the original stack.</param>
		/// <returns><see cref="true"/> if at least 1 item was added. Otherwise <see cref="false"/>.</returns>
		bool TryAdd(in ItemStack stack, out ItemStack remainder);

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
		/// Try to add an <see cref="ItemStack"/> to a specific slot.
		/// </summary>
		/// <param name="index">The slot index to add to.</param>
		/// <param name="stack">The <see cref="ItemStack"/> to add.</param>
		/// <param name="result">The resulting <see cref="ItemStack"/> at <paramref name="index"/>, after adding.</param>
		/// <param name="remainder">If <paramref name="stack"/> could not be added in its entirety, this will contain how much remains of the original stack.</param>
		/// <returns><see cref="true"/> if at least 1 item was added. Otherwise <see cref="false"/>.</returns>
		bool TryAddSlot(int index, in ItemStack stack, out ItemStack result, out ItemStack remainder);

		/// <summary>
		/// Check if it's possible to remove a certain amount of a specific item from the container.
		/// </summary>
		/// <param name="stack">The <see cref="ItemStack"/> to remove.</param>
		/// <returns><see cref="true"/> if the container has enough of the specified item and it can be removed. Otherwise <see cref="false"/>.</returns>
		bool CanRemove(in ItemStack stack);

		/// <summary>
		/// Try to remove a certain amount of a specific item from the container.
		/// </summary>
		/// <param name="stack">The <see cref="ItemStack"/> to remove.</param>
		/// <returns><see cref="true"/> if the container had enough of the specified item and it was removed. Otherwise <see cref="false"/>.</returns>
		bool TryRemove(in ItemStack stack);

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
		/// Try to remove a certain amount of items from a specific slot.
		/// </summary>
		/// <param name="index">The slot index to remove from.</param>
		/// <param name="result">The resulting <see cref="ItemStack"/> at <paramref name="index"/>, after removing.</param>
		/// <param name="removed">The <see cref="ItemStack"/> that was removed.</param>
		/// <param name="count">The amount of items to remove. Passing in a negative number removes the entire stack.</param>
		/// <returns><see cref="true"/> if the slot had enough of the specified item and it was removed. Otherwise <see cref="false"/>.</returns>
		bool TryRemoveSlot(int index, out ItemStack result, out ItemStack removed, int count = -1);

		/// <summary>
		/// Check if it's possible to move a certain amount of items from a specific slot to another <see cref="IItemContainer"/>.
		/// </summary>
		/// <param name="fromIndex">The slot index to move from.</param>
		/// <param name="to">The <see cref="IItemContainer"/> to move to.</param>
		/// <param name="count">The amount of items to move. Passing in a negative number moves the entire stack.</param>
		/// <returns><see cref="true"/> if it is possible to move the specified amount of items. Otherwise <see cref="false"/>.</returns>
		bool CanMove(int fromIndex, IItemContainer to, int count = -1);

		/// <summary>
		/// Try to move a certain amount of items from a specific slot to another <see cref="IItemContainer"/>.
		/// </summary>
		/// <param name="fromIndex">The slot index to move from.</param>
		/// <param name="to">The <see cref="IItemContainer"/> to move to.</param>
		/// <param name="count">The amount of items to move. Passing in a negative number moves the entire stack.</param>
		/// <returns><see cref="true"/> if it was possible to move the specified amount of items. Otherwise <see cref="false"/>.</returns>
		bool TryMove(int fromIndex, IItemContainer to, int count = -1);

		/// <summary>
		/// Check if it's possible to move a certain amount of items from a specific slot to another slot of an <see cref="IItemContainer"/>.
		/// </summary>
		/// <param name="fromIndex">The slot index to move from.</param>
		/// <param name="to">The <see cref="IItemContainer"/> to move to.</param>
		/// <param name="toIndex">The slot index of the target <see cref="IItemContainer"/> to move to.</param>
		/// <param name="count">The amount of items to move. Passing in a negative number moves the entire stack.</param>
		/// <returns><see cref="true"/> if it is possible to move the specified amount of items. Otherwise <see cref="false"/>.</returns>
		bool CanMoveSlot(int fromIndex, IItemContainer to, int toIndex, int count = -1);

		/// <summary>
		/// Try to move a certain amount of items from a specific slot to another slot of an <see cref="IItemContainer"/>.
		/// </summary>
		/// <param name="fromIndex">The slot index to move from.</param>
		/// <param name="to">The <see cref="IItemContainer"/> to move to.</param>
		/// <param name="toIndex">The slot index of the target <see cref="IItemContainer"/> to move to.</param>
		/// <param name="count">The amount of items to move. Passing in a negative number moves the entire stack.</param>
		/// <returns><see cref="true"/> if it was possible to move the specified amount of items. Otherwise <see cref="false"/>.</returns>
		bool TryMoveSlot(int fromIndex, IItemContainer to, int toIndex, int count = -1);

		/// <summary>
		/// Check if it's possible to set the size of the container.
		/// </summary>
		/// <param name="value">The size to set.</param>
		/// <returns><see cref="true"/> if the size can be set. Otherwise <see cref="false"/>.</returns>
		bool CanSetSize(int value);

		/// <summary>
		/// Try to set the size of the container.
		/// </summary>
		/// <param name="value">The size to set.</param>
		/// <returns><see cref="true"/> if the size was set successfully. Otherwise <see cref="false"/>.</returns>
		bool TrySetSize(int value);

		/// <summary>
		/// <para>Overwrite all slots by copying from a <see cref="ReadOnlySpan{T}"/> of <see cref="ItemStack"/>.</para>
		/// <para>If the current <see cref="Size"/> is smaller than the provided <see cref="ReadOnlySpan{T}.Length"/>, <see cref="Size"/> will increase to match the span's size, otherwise it will stay the same.</para>
		/// </summary>
		/// <param name="stacks">The <see cref="ItemStack"/>s to copy from.</param>
		void CopyFrom(ReadOnlySpan<ItemStack> stacks);

		/// <summary>
		/// <para>Overwrite all slots by copying from a <see cref="IReadOnlyList{T}"/> of <see cref="ItemStack"/>.</para>
		/// <para>If the current <see cref="Size"/> is smaller than the provided IReadOnlyList.Count, <see cref="Size"/> will increase to match the list's size, otherwise it will stay the same.</para>
		/// </summary>
		/// <param name="stacks">The <see cref="ItemStack"/>s to copy from.</param>
		void CopyFrom(IReadOnlyList<ItemStack> stacks);

		/// <summary>
		/// Clear the container, emptying all slots.
		/// </summary>
		void Clear();

		void Sort();

		void Sort<TComparer>(TComparer comparer) where TComparer : IComparer<ItemStack>;

		public static bool TryMove<T>(T transaction, IItemContainer from, int fromIndex, IItemContainer to, int count = -1) where T : IItemTransaction
		{
			if (!from.Transactions.TryRemoveSlot(transaction, fromIndex, out _, out ItemStack removed, count))
			{
				return false;
			}

			if (!to.Transactions.TryAdd(transaction, removed, out ItemStack remainder))
			{
				return false;
			}

			if (!remainder.IsEmpty && (!from.Transactions.TryAddSlot(transaction, fromIndex, remainder, out _, out ItemStack rem) || !rem.IsEmpty))
			{
				return false;
			}

			return true;
		}

		public static bool TryMoveSlot<T>(T transaction, IItemContainer from, int fromIndex, IItemContainer to, int toIndex, int count = -1) where T : IItemTransaction
		{
			if (!from.Transactions.TryRemoveSlot(transaction, fromIndex, out ItemStack result, out ItemStack fromSlot, count))
			{
				return false;
			}

			if (to.Transactions.TryAddSlot(transaction, toIndex, fromSlot, out _, out ItemStack remainder))
			{
				if (remainder.IsEmpty || from.Transactions.TryAddSlot(transaction, fromIndex, remainder, out _, out remainder) && remainder.IsEmpty)
				{
					return true;
				}
			}
			else if (result.IsEmpty && to.Transactions.TryRemoveSlot(transaction, toIndex, out _, out ItemStack toSlot))
			{
				if (!from.Transactions.TryAddSlot(transaction, fromIndex, toSlot, out _, out remainder) || !remainder.IsEmpty)
				{
					return false;
				}

				if (!to.Transactions.TryAddSlot(transaction, toIndex, fromSlot, out _, out remainder) || !remainder.IsEmpty)
				{
					return false;
				}

				return true;
			}

			return false;
		}
	}
}