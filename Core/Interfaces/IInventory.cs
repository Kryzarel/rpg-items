using System;
using System.Collections.Generic;

namespace Kryz.RPG.Items
{
	public interface IInventory : IReadOnlyList<ItemStack>
	{
		public delegate void OnSlotChangedDelegate(int index, ItemStack previousStack, ItemStack newStack);
		public delegate void OnSizeChangedDelegate(int previousSize, int newSize);

		event OnSlotChangedDelegate OnSlotChanged;
		event OnSizeChangedDelegate OnSizeChanged;

		int Size { get; set; }
		int FreeSlots { get; }
		int OccupiedSlots { get; }

		/// <summary>
		/// Get the maximum stack size of a particular item.
		/// </summary>
		/// <param name="item">The item to check.</param>
		/// <returns>The maximum stack size for the <paramref name="item"/>.</returns>
		int GetMaxStacks(IItem item);

		/// <summary>
		/// Check if an item slot can be swapped with the specified <see cref="ItemStack"/>.
		/// </summary>
		/// <param name="index">The slot index to set.</param>
		/// <param name="stack">The <see cref="ItemStack"/> to assign to the slot.</param>
		/// <param name="removed">The <see cref="ItemStack"/> that will be removed from the slot.</param>
		/// <returns>True if the inventory slot can be set to the specified <see cref="ItemStack"/>. False otherwise.</returns>
		bool CanSwap(int index, in ItemStack stack, out ItemStack removed);

		/// <summary>
		/// Check if it's possible to add an <see cref="ItemStack"/> to the inventory.
		/// </summary>
		/// <param name="stack">The item stack to add.</param>
		/// <param name="remainder">If <paramref name="stack"/> cannot be added in its entirety, this will contain how much will remain of the original stack.</param>
		/// <param name="allowPartial">Whether to allow only a portion of the stack to be added.</param>
		/// <returns>True if the inventory has enough space for the whole stack, or at least 1 item, if '<paramref name="allowPartial"/>' is true. False otherwise.</returns>
		bool CanAdd(in ItemStack stack, out ItemStack remainder, bool allowPartial = false);

		/// <summary>
		/// Check if it's possible to add an <see cref="ItemStack"/> to a specific inventory slot.
		/// </summary>
		/// <param name="stack">The item stack to add.</param>
		/// <param name="result">The resulting item stack at <see cref="index"/>, after adding.</param>
		/// <param name="remainder">If <paramref name="stack"/> cannot be added in its entirety, this will contain how much will remain of the original stack.</param>
		/// <param name="allowPartial">Whether to allow only a portion of the stack to be added.</param>
		/// <returns>True if the slot has enough space for the whole stack, or at least 1 item, if '<paramref name="allowPartial"/>' is true. False otherwise.</returns>
		bool CanAdd(int index, in ItemStack stack, out ItemStack result, out ItemStack remainder, bool allowPartial = false);

		/// <summary>
		/// Check if the it's possible to remove an <see cref="ItemStack"/> from the inventory.
		/// </summary>
		/// <param name="stack">The <see cref="ItemStack"/> to remove.</param>
		/// <returns>True if the inventory has enough of the specified item and it can be removed. False otherwise.</returns>
		bool CanRemove(in ItemStack stack);

		/// <summary>
		/// Check if the it's possible to remove the whole item stack from a specific inventory slot.
		/// </summary>
		/// <param name="index">The slot index to remove from.</param>
		/// <param name="result">The resulting item stack at <see cref="index"/>, after removing.</param>
		/// <param name="removed">The item stack that would be removed.</param>
		/// <returns>True if the <see cref="ItemStack"/> in the slot can be removed. False otherwise.</returns>
		bool CanRemove(int index, out ItemStack result, out ItemStack removed);

		/// <summary>
		/// Check if the it's possible to remove a certain amount of items from a specific inventory slot.
		/// </summary>
		/// <param name="index">The slot index to remove from.</param>
		/// <param name="count">The amount to remove.</param>
		/// <param name="result">The resulting item stack at <see cref="index"/>, after removing.</param>
		/// <param name="removed">The item stack that would be removed.</param>
		/// <returns>True if the slot has enough items and they can be removed. False otherwise.</returns>
		bool CanRemove(int index, int count, out ItemStack result, out ItemStack removed);

		/// <summary>
		/// Try to swap an item slot with the specified <see cref="ItemStack"/>.
		/// </summary>
		/// <param name="index">The slot index to set.</param>
		/// <param name="stack">The <see cref="ItemStack"/> to assign to the slot.</param>
		/// <param name="removed">The <see cref="ItemStack"/> that was removed from the slot.</param>
		/// <param name="forceSwap">Force swap the slot, even if under normal conditions it wouldn't be allowed.</param>
		/// <returns>False if the slot is already equal to '<paramref name="stack"/>' or if it can't be set to the specified <see cref="ItemStack"/>. True otherwise.</returns>
		bool TrySwap(int index, in ItemStack stack, out ItemStack removed, bool forceSwap = false);

		/// <summary>
		/// Try to add an item stack to any inventory slots that can receive it.
		/// </summary>
		/// <param name="stack">The item stack to add.</param>
		/// <param name="remainder">If <paramref name="stack"/> could not be added in its entirety, this will contain how much remains of the original stack.</param>
		/// <param name="allowPartial">Whether to allow only a portion of the stack to be added.</param>
		/// <returns>True if the inventory had enough space for the whole stack, or at least 1 item, if '<paramref name="allowPartial"/>' is true. False otherwise.</returns>
		bool TryAdd(in ItemStack stack, out ItemStack remainder, bool allowPartial = false);

		/// <summary>
		/// Try to add an item stack to a specific inventory slot.
		/// </summary>
		/// <param name="index">The slot index to add to.</param>
		/// <param name="stack">The item stack to add.</param>
		/// <param name="result">The resulting item stack at <see cref="index"/>, after adding.</param>
		/// <param name="remainder">If <paramref name="stack"/> could not be added in its entirety, this will contain how much remains of the original stack.</param>
		/// <param name="allowPartial">Whether to allow only a portion of the stack to be added.</param>
		/// <returns>True if the slot had enough space for the whole stack, or at least 1 item, if '<paramref name="allowPartial"/>' is true. False otherwise.</returns>
		bool TryAdd(int index, in ItemStack stack, out ItemStack result, out ItemStack remainder, bool allowPartial = false);

		/// <summary>
		/// Try to remove a certain amount of a specific item from the inventory.
		/// </summary>
		/// <param name="stack">The item stack to remove.</param>
		/// <returns>True if the inventory had enough of the specified item and it was removed. False otherwise.</returns>
		bool TryRemove(in ItemStack stack);

		/// <summary>
		/// Try to remove an entire item stack from a specific inventory slot.
		/// </summary>
		/// <param name="index">The slot index to remove from.</param>
		/// <param name="result">The resulting item stack at <see cref="index"/>, after removing.</param>
		/// <param name="removed">The item stack that was removed.</param>
		/// <returns>True if the slot had a non-empty item stack and it was removed. False otherwise.</returns>
		bool TryRemove(int index, out ItemStack result, out ItemStack removed);

		/// <summary>
		/// Try to remove a certain amount of items from a specific inventory slot.
		/// </summary>
		/// <param name="index">The slot index to remove from.</param>
		/// <param name="count">The amount of items to remove.</param>
		/// <param name="result">The resulting item stack at <see cref="index"/>, after removing.</param>
		/// <param name="removed">The item stack that was removed.</param>
		/// <returns>True if the slot had an item stack with enough items and it was removed. False otherwise.</returns>
		bool TryRemove(int index, int count, out ItemStack result, out ItemStack removed);

		bool CanMove(int toIndex, IInventory fromInv, int fromIndex, bool allowSwap = false) => CanMove(this, toIndex, fromInv, fromIndex, allowSwap);

		bool TryMove(int toIndex, IInventory fromInv, int fromIndex, bool allowSwap = false) => TryMove(this, toIndex, fromInv, fromIndex, allowSwap);

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

		void Sort();

		void Sort<TComparer>(TComparer comparer) where TComparer : IComparer<ItemStack>;

		void Clear();

		static bool CanMove(IInventory toInv, int toIndex, IInventory fromInv, int fromIndex, bool allowSwap = false)
		{
			if (toIndex == fromIndex && toInv == fromInv || !fromInv.CanRemove(fromIndex, out _, out ItemStack fromSlot))
			{
				return false;
			}

			if (toInv.CanAdd(toIndex, fromSlot, out _, out ItemStack remainder, allowPartial: true) && fromInv.CanSwap(fromIndex, remainder, out _))
			{
				return true;
			}
			else if (allowSwap && toInv.CanSwap(toIndex, fromSlot, out ItemStack toSlot) && fromInv.CanSwap(fromIndex, toSlot, out _))
			{
				return true;
			}

			return false;
		}

		static bool TryMove(IInventory toInv, int toIndex, IInventory fromInv, int fromIndex, bool allowSwap = false)
		{
			if (toIndex == fromIndex && toInv == fromInv || !fromInv.CanRemove(fromIndex, out _, out ItemStack fromSlot))
			{
				return false;
			}

			if (toInv.CanAdd(toIndex, fromSlot, out _, out ItemStack remainder, allowPartial: true) && fromInv.CanSwap(fromIndex, remainder, out _))
			{
				toInv.TryAdd(toIndex, fromSlot, out _, out remainder, allowPartial: true);
				fromInv.TrySwap(fromIndex, remainder, out _);
				return true;
			}
			else if (allowSwap && toInv.CanSwap(toIndex, fromSlot, out ItemStack toSlot) && fromInv.CanSwap(fromIndex, toSlot, out _))
			{
				toInv.TrySwap(toIndex, fromSlot, out toSlot);
				fromInv.TrySwap(fromIndex, toSlot, out _);
				return true;
			}

			return false;
		}
	}
}