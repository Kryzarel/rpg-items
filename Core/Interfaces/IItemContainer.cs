using System;
using System.Collections.Generic;

namespace Kryz.RPG.Items
{
	public interface IItemContainer : IReadOnlyList<ItemStack>
	{
		public delegate void OnSlotChangedDelegate(int index, ItemStack oldStack, ItemStack newStack);
		public delegate void OnSizeChangedDelegate(int oldSize, int newSize);

		event OnSlotChangedDelegate OnSlotChanged;
		event OnSizeChangedDelegate OnSizeChanged;

		public IItemContainerStrategy Strategy { get; set; }

		public ReadOnlySpan<ItemStack> Slots { get; }

		int Size { get; }
		int FreeSlots { get; }
		int OccupiedSlots { get; }

		bool TrySetSize(int value);

		/// <summary>
		/// Try to add an item stack to any inventory slots that can receive it.
		/// </summary>
		/// <param name="stack">The item stack to add.</param>
		/// <param name="remainder">If <paramref name="stack"/> could not be added in its entirety, this will contain how much remains of the original stack.</param>
		/// <returns><see cref="true"/> if the inventory had enough space for at least 1 item. Otherwise <see cref="false"/>.</returns>
		bool TryAdd(in ItemStack stack, out ItemStack remainder);

		/// <summary>
		/// Try to add an item stack to a specific inventory slot.
		/// </summary>
		/// <param name="index">The slot index to add to.</param>
		/// <param name="stack">The item stack to add.</param>
		/// <param name="result">The resulting item stack at <paramref name="index"/>, after adding.</param>
		/// <param name="remainder">If <paramref name="stack"/> could not be added in its entirety, this will contain how much remains of the original stack.</param>
		/// <returns><see cref="true"/> if the slot had enough space for at least 1 item. Otherwise <see cref="false"/>.</returns>
		bool TryAdd(int index, in ItemStack stack, out ItemStack result, out ItemStack remainder);

		/// <summary>
		/// Try to remove a certain amount of a specific item from the inventory.
		/// </summary>
		/// <param name="stack">The item stack to remove.</param>
		/// <returns><see cref="true"/> if the inventory had enough of the specified item and it was removed. Otherwise <see cref="false"/>.</returns>
		bool TryRemove(in ItemStack stack);

		/// <summary>
		/// Try to remove a certain amount of items from a specific inventory slot.
		/// </summary>
		/// <param name="index">The slot index to remove from.</param>
		/// <param name="count">The amount of items to remove. Passing in a negative number removes the entire stack.</param>
		/// <param name="result">The resulting item stack at <paramref name="index"/>, after adding.</param>
		/// <param name="removed">The item stack that was removed.</param>
		/// <returns><see cref="true"/> if the slot had enough of the specified item and it was removed. Otherwise <see cref="false"/>.</returns>
		bool TryRemove(int index, int count, out ItemStack result, out ItemStack removed);

		/// <summary>
		/// Try to move a certain amount of items from a specific inventory slot to another <see cref="IItemContainer"/>.
		/// </summary>
		/// <param name="fromIndex">The slot index to move from.</param>
		/// <param name="count">The amount of items to move. Passing in a negative number moves the entire stack.</param>
		/// <param name="to">The <see cref="IItemContainer"/> to move to.</param>
		/// <returns><see cref="true"/> if it was possible to move the specified amount of items. Otherwise <see cref="false"/>.</returns>
		bool TryMove(int fromIndex, int count, IItemContainer to);

		/// <summary>
		/// Try to move a certain amount of items from a specific inventory slot to another slot of an <see cref="IItemContainer"/>.
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

		void Commit(ReadOnlySpan<SlotEdit> slotEdits, SizeEdit? sizeEdit);
	}
}