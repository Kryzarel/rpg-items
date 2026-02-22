using System.Collections.Generic;

namespace Kryz.RPG.Items
{
	public interface IItemContainerTransactional : IReadOnlyItemContainer
	{
		public delegate void OnSizeChangedDelegate(int oldSize, int newSize);
		public delegate void OnSlotChangedDelegate(int index, ItemStack oldStack, ItemStack newStack);
		public delegate void OnTransactionSuccessDelegate();

		event OnSizeChangedDelegate OnSizeChanged;
		event OnSlotChangedDelegate OnSlotChanged;
		event OnTransactionSuccessDelegate OnTransactionSuccess;

		int InstanceId { get; }

		/// <summary>
		/// Get the maximum stack size of a particular item.
		/// </summary>
		/// <param name="item">The item to check.</param>
		/// <returns>The maximum stack size for the <paramref name="item"/>.</returns>
		int GetMaxStacks(IItem item);

		/// <summary>
		/// Check if the slot at <paramref name="index"/> can receive the specified <see cref="IItem"/>.
		/// </summary>
		/// <param name="index">The slot index to check.</param>
		/// <param name="item">The <see cref="IItem"/> to check.</param>
		/// <returns><see cref="true"/> if the slot can receive the <paramref name="stack"/>. Otherwise <see cref="false"/>.</returns>
		bool CanReceive(int index, IItem item);

		/// <summary>
		/// Check if the slot at <paramref name="index"/> can remove any of its contents.
		/// </summary>
		/// <param name="index">The slot index to check.</param>
		/// <returns><see cref="true"/> items can be removed from the slot. Otherwise <see cref="false"/>.</returns>
		bool CanRemove(int index);

		/// <summary>
		/// Check if it's possible to add an <see cref="ItemStack"/> to the container.
		/// </summary>
		/// <param name="stack">The <see cref="ItemStack"/> to add.</param>
		/// <param name="remainder">If <paramref name="stack"/> cannot be added in its entirety, this will contain how much remains of the original stack.</param>
		/// <returns><see cref="true"/> if at least 1 item can be added. Otherwise <see cref="false"/>.</returns>
		bool CanAdd(in ItemStack stack, out ItemStack remainder);

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
		/// Check if it's possible to remove a certain amount of a specific item from the container.
		/// </summary>
		/// <param name="stack">The <see cref="ItemStack"/> to remove.</param>
		/// <returns><see cref="true"/> if the container has enough of the specified item and it can be removed. Otherwise <see cref="false"/>.</returns>
		bool CanRemove(in ItemStack stack);

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
		/// Check if it's possible to move a certain amount of items from a specific slot to another <see cref="IItemContainer"/>.
		/// </summary>
		/// <param name="fromIndex">The slot index to move from.</param>
		/// <param name="to">The <see cref="IItemContainer"/> to move to.</param>
		/// <param name="count">The amount of items to move. Passing in a negative number moves the entire stack.</param>
		/// <returns><see cref="true"/> if it is possible to move the specified amount of items. Otherwise <see cref="false"/>.</returns>
		public bool CanMove(int fromIndex, IItemContainer to, int count = -1);

		/// <summary>
		/// Check if it's possible to move a certain amount of items from a specific slot to another slot of an <see cref="IItemContainer"/>.
		/// </summary>
		/// <param name="fromIndex">The slot index to move from.</param>
		/// <param name="to">The <see cref="IItemContainer"/> to move to.</param>
		/// <param name="toIndex">The slot index of the target <see cref="IItemContainer"/> to move to.</param>
		/// <param name="count">The amount of items to move. Passing in a negative number moves the entire stack.</param>
		/// <returns><see cref="true"/> if it is possible to move the specified amount of items. Otherwise <see cref="false"/>.</returns>
		public bool CanMoveSlot(int fromIndex, IItemContainer to, int toIndex, int count = -1);

		/// <summary>
		/// Check if it's possible to set a slot to the specified <see cref="ItemStack"/>.
		/// </summary>
		/// <param name="index">The slot index to set.</param>
		/// <param name="stack">The stack to set the slot to.</param>
		/// <returns><see cref="true"/> if it is possible to set the slot. Otherwise <see cref="false"/>.</returns>
		bool CanSetSlot(int index, in ItemStack stack);

		/// <summary>
		/// Check if it's possible to set the size of the container.
		/// </summary>
		/// <param name="value">The size to set.</param>
		/// <returns><see cref="true"/> if the size can be set. Otherwise <see cref="false"/>.</returns>
		bool CanSetSize(int value);

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
		/// Try to set a slot to the specified <see cref="ItemStack"/>.
		/// </summary>
		/// <param name="index">The slot index to set.</param>
		/// <param name="stack">The stack to set the slot to.</param>
		/// <returns><see cref="true"/> if it was possible to set the slot. Otherwise <see cref="false"/>.</returns>
		bool TrySetSlot<T>(T transaction, int index, in ItemStack stack) where T : IItemTransaction;

		/// <summary>
		/// Try to set the container to the specified size.
		/// </summary>
		/// <param name="value">The size to set.</param>
		/// <returns><see cref="true"/> if it was possible to resize the container. Otherwise <see cref="false"/>.</returns>
		bool TrySetSize<T>(T transaction, int value) where T : IItemTransaction;

		/// <summary>
		/// <para>Set a slot to the specified <see cref="ItemStack"/>. Sets the slot whether the Container allows it or not.</para>
		/// <para>If you want to obey the Container's rules use <see cref="TrySetSlot"/> instead.</para>
		/// </summary>
		/// <param name="index">The slot index to set.</param>
		/// <param name="stack">The stack to set the slot to.</param>
		/// <returns><see cref="true"/> if it was possible to set the slot. Otherwise <see cref="false"/>.</returns>
		void SetSlot<T>(T transaction, int index, in ItemStack stack) where T : IItemTransaction;

		void Sort<T>(T transaction) where T : IItemTransaction;

		void Sort<T, TComparer>(T transaction, TComparer comparer) where TComparer : IComparer<ItemStack> where T : IItemTransaction;

		/// <summary>
		/// Notify observers of a size change after a successful transaction.
		/// </summary>
		/// <param name="oldSize">The old size</param>
		/// <param name="newSize">The new size</param>
		void NotifySize(int oldSize, int newSize);

		/// <summary>
		/// Notify observers of a slot change after a successful transaction.
		/// </summary>
		/// <param name="index">The changed slot index</param>
		/// <param name="oldStack">The old slot</param>
		/// <param name="newStack">The new slot</param>
		void NotifySlot(int index, ItemStack oldStack, ItemStack newStack);

		/// <summary>
		/// Notify observers at the end of a successful transaction.
		/// </summary>
		void NotifyTransactionSuccess();

		/// <summary>
		/// Rollback a slot change after a failed transaction.
		/// </summary>
		/// <param name="oldSize">The old size</param>
		void RollbackSize(int oldSize);

		/// <summary>
		/// Rollback a slot change after a failed transaction.
		/// </summary>
		/// <param name="index">The slot index</param>
		/// <param name="oldStack">The old slot</param>
		void RollbackSlot(int index, ItemStack oldStack);
	}
}