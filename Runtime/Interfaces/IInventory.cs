using System;
using System.Collections.Generic;
using Kryz.RPG.Items.Structs;

namespace Kryz.RPG.Items.Interfaces
{
	public interface IInventory : IReadOnlyList<ItemStack>
	{
		public delegate void OnSlotChangedDelegate(int index, ItemStack previousStack, ItemStack newStack);
		public delegate void OnSizeChangedDelegate(int previousValue, int newValue);

		event OnSlotChangedDelegate OnSlotChanged;
		event OnSizeChangedDelegate OnSizeChanged;

		int Size { get; set; }
		int FreeSlots { get; }
		int OccupiedSlots { get; }

		/// <summary>
		/// Check if an item slot can be set to the specified <see cref="ItemStack"/>.
		/// </summary>
		/// <param name="index">The slot index to set.</param>
		/// <param name="stack">The <see cref="ItemStack"/> to assign to the slot.</param>
		/// <returns>True if the inventory slot can be set to the specified <see cref="ItemStack"/>. False otherwise.</returns>
		bool CanSet(int index, in ItemStack stack);

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
		/// <param name="remainder">If <paramref name="stack"/> cannot be added in its entirety, this will contain how much will remain of the original stack.</param>
		/// <param name="allowPartial">Whether to allow only a portion of the stack to be added.</param>
		/// <returns>True if the slot has enough space for the whole stack, or at least 1 item, if '<paramref name="allowPartial"/>' is true. False otherwise.</returns>
		bool CanAdd(int index, in ItemStack stack, out ItemStack remainder, bool allowPartial = false);

		/// <summary>
		/// Check if the inventory contains a certain amount of an item.
		/// </summary>
		/// <param name="stack">The <see cref="ItemStack"/> to check.</param>
		/// <returns>True if the inventory has enough of the specified item. False otherwise.</returns>
		bool Contains(in ItemStack stack);

		/// <summary>
		/// Try to set an item slot to the specified <see cref="ItemStack"/>, overwriting any <see cref="ItemStack"/> that may already be in that slot.
		/// </summary>
		/// <param name="index">The slot index to set.</param>
		/// <param name="stack">The <see cref="ItemStack"/> to assign to the slot.</param>
		/// <param name="forceSet">Force set the slot, even if under normal conditions it wouldn't be allowed.</param>
		/// <returns>False if the slot is already equal to '<paramref name="stack"/>' or if it can't be set to the specified <see cref="ItemStack"/>. True otherwise.</returns>
		bool TrySet(int index, in ItemStack stack, bool forceSet = false);

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
		/// <param name="remainder">If <paramref name="stack"/> could not be added in its entirety, this will contain how much remains of the original stack.</param>
		/// <param name="allowPartial">Whether to allow only a portion of the stack to be added.</param>
		/// <returns>True if the slot had enough space for the whole stack, or at least 1 item, if '<paramref name="allowPartial"/>' is true. False otherwise.</returns>
		bool TryAdd(int index, in ItemStack stack, out ItemStack remainder, bool allowPartial = false);

		/// <summary>
		/// Try to remove a certain amount of a specific item from the inventory.
		/// </summary>
		/// <param name="stack">The item stack to remove.</param>
		/// <returns>True if the inventory had enough of the specified item and it was removed. False otherwise.</returns>
		bool TryRemove(in ItemStack stack);

		/// <summary>
		/// Try to remove a certain amount of items from a specific inventory slot.
		/// </summary>
		/// <param name="index">The slot index to remove from.</param>
		/// <param name="count">The amount of items to remove.</param>
		/// <param name="removed">The item stack that was removed.</param>
		/// <returns>True if the slot had a valid item stack and it was removed. False otherwise.</returns>
		bool TryRemove(int index, int count, out ItemStack removed);

		/// <summary>
		/// Try to remove an entire item stack from a specific inventory slot.
		/// </summary>
		/// <param name="index">The slot index to remove from.</param>
		/// <param name="removed">The item stack that was removed.</param>
		/// <returns>True if the slot had a valid item stack and it was removed. False otherwise.</returns>
		bool TryRemove(int index, out ItemStack removed);

		/// <summary>
		/// <para>Try to move an item stack from a specific inventory slot to another.</para>
		/// <para>If the '<paramref name="toIndex"/>' slot can accept the item stack (same item and enough space), it is simply moved.</para>
		/// <para>If '<paramref name="toIndex"/>' can't accept the item AND '<paramref name="allowSwap"/>' is true, the '<paramref name="toIndex"/>' and '<paramref name="fromIndex"/>' item stacks are swapped. Otherwise nothing happens.</para>
		/// </summary>
		/// <param name="toIndex">The slot index to move to.</param>
		/// <param name="fromIndex">The slot index to move from.</param>
		/// <param name="fromInventory">The inventory to move from.</param>
		/// <param name="allowSwap">If we should swap the stacks when the target inventory doesn't have enough space.</param>
		/// <returns>True if '<paramref name="fromIndex"/>' has a valid item stack and it was successfully moved to or swapped with '<paramref name="toIndex"/>'. False otherwise.</returns>
		bool TryMove(int toIndex, int fromIndex, IInventory fromInventory, bool allowSwap = false);

		/// <summary>
		/// <para>Overwrite all slots by copying from a <see cref="ReadOnlySpan{T}"/> of <see cref="ItemStack"/>.</para>
		/// <para>If the current <see cref="Size"/> is smaller than the provided list, it will increase to match the list's size, otherwise, <see cref="Size"/> will stay the same.</para>
		/// </summary>
		/// <param name="stacks">The <see cref="ItemStack"/>s to copy from.</param>
		void CopyFrom(ReadOnlySpan<ItemStack> stacks);

		/// <summary>
		/// <para>Overwrite all slots by copying from a <see cref="IReadOnlyList{T}"/> of <see cref="ItemStack"/>.</para>
		/// <para>If the current <see cref="Size"/> is smaller than the provided list, it will increase to match the list's size, otherwise, <see cref="Size"/> will stay the same.</para>
		/// </summary>
		/// <param name="stacks">The <see cref="ItemStack"/>s to copy from.</param>
		void CopyFrom(IReadOnlyList<ItemStack> stacks);

		void Sort();
		void Sort<TComparer>(TComparer comparer) where TComparer : IComparer<ItemStack>;

		void Clear();
	}
}