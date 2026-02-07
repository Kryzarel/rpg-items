using System;
using System.Collections.Generic;

namespace Kryz.RPG.Items
{
	public interface IItemContainer : IItemContainerTransactions, IReadOnlyList<ItemStack>
	{
		public delegate void OnSlotChangedDelegate(int index, ItemStack stack);
		public delegate void OnSizeChangedDelegate(int size);

		event OnSlotChangedDelegate OnSlotChanged;
		event OnSizeChangedDelegate OnSizeChanged;

		public ReadOnlySpan<ItemStack> Slots { get; }

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
		/// Try to set the size of the container.
		/// </summary>
		/// <param name="value">The size to set.</param>
		/// <returns><see cref="true"/> if the size was set successfully. Otherwise <see cref="false"/>.</returns>
		bool TrySetSize(int value);

		/// <summary>
		/// Try to add an item stack to any slots that can receive it.
		/// </summary>
		/// <param name="stack">The item stack to add.</param>
		/// <param name="remainder">If <paramref name="stack"/> could not be added in its entirety, this will contain how much remains of the original stack.</param>
		/// <returns><see cref="true"/> if the container had enough space for at least 1 item. Otherwise <see cref="false"/>.</returns>
		bool TryAdd(in ItemStack stack, out ItemStack remainder);

		/// <summary>
		/// Try to add an item stack to a specific slot.
		/// </summary>
		/// <param name="index">The slot index to add to.</param>
		/// <param name="stack">The item stack to add.</param>
		/// <param name="result">The resulting item stack at <paramref name="index"/>, after adding.</param>
		/// <param name="remainder">If <paramref name="stack"/> could not be added in its entirety, this will contain how much remains of the original stack.</param>
		/// <returns><see cref="true"/> if the slot had enough space for at least 1 item. Otherwise <see cref="false"/>.</returns>
		bool TryAdd(int index, in ItemStack stack, out ItemStack result, out ItemStack remainder);

		/// <summary>
		/// Try to remove a certain amount of a specific item from the container.
		/// </summary>
		/// <param name="stack">The item stack to remove.</param>
		/// <returns><see cref="true"/> if the container had enough of the specified item and it was removed. Otherwise <see cref="false"/>.</returns>
		bool TryRemove(in ItemStack stack);

		/// <summary>
		/// Try to remove a certain amount of items from a specific slot.
		/// </summary>
		/// <param name="index">The slot index to remove from.</param>
		/// <param name="count">The amount of items to remove. Passing in a negative number removes the entire stack.</param>
		/// <param name="result">The resulting item stack at <paramref name="index"/>, after adding.</param>
		/// <param name="removed">The item stack that was removed.</param>
		/// <returns><see cref="true"/> if the slot had enough of the specified item and it was removed. Otherwise <see cref="false"/>.</returns>
		bool TryRemove(int index, int count, out ItemStack result, out ItemStack removed);

		/// <summary>
		/// Try to move a certain amount of items from a specific slot to another <see cref="IItemContainer"/>.
		/// </summary>
		/// <param name="fromIndex">The slot index to move from.</param>
		/// <param name="count">The amount of items to move. Passing in a negative number moves the entire stack.</param>
		/// <param name="to">The <see cref="IItemContainer"/> to move to.</param>
		/// <returns><see cref="true"/> if it was possible to move the specified amount of items. Otherwise <see cref="false"/>.</returns>
		bool TryMove(int fromIndex, int count, IItemContainer to);

		/// <summary>
		/// Try to move a certain amount of items from a specific slot to another slot of an <see cref="IItemContainer"/>.
		/// </summary>
		/// <param name="fromIndex">The slot index to move from.</param>
		/// <param name="count">The amount of items to move. Passing in a negative number moves the entire stack.</param>
		/// <param name="to">The <see cref="IItemContainer"/> to move to.</param>
		/// <param name="toIndex">The slot index of the target <see cref="IItemContainer"/> to move to.</param>
		/// <returns><see cref="true"/> if it was possible to move the specified amount of items. Otherwise <see cref="false"/>.</returns>
		bool TryMove(int fromIndex, int count, IItemContainer to, int toIndex);

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

		void Clear();

		void Sort();

		void Sort<TComparer>(TComparer comparer) where TComparer : IComparer<ItemStack>;
	}
}