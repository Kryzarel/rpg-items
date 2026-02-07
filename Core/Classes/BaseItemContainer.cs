using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using Kryz.Utils;

namespace Kryz.RPG.Items
{
	public abstract class BaseItemContainer : IItemContainer
	{
		protected readonly ArrayPool<ItemStack> arrayPool;

		protected IItemContainerStrategy strategy;
		protected ItemStack[] slots;
		protected int size;
		protected int occupiedSlots;

		public virtual ItemStack this[int index] => index >= 0 && index < size ? slots[index] : throw new IndexOutOfRangeException();
		public virtual ReadOnlySpan<ItemStack> Slots => slots[..size];
		public virtual IItemContainerStrategy Strategy { get => strategy; set => strategy = value; }

		public virtual int Count => size;
		public virtual int Size { get => size; set => SetSize(value); }
		public virtual int FreeSlots => size - occupiedSlots;
		public virtual int OccupiedSlots => occupiedSlots;

		public virtual event IItemContainer.OnSlotChangedDelegate OnSlotChanged = delegate { };
		public virtual event IItemContainer.OnSizeChangedDelegate OnSizeChanged = delegate { };

		public BaseItemContainer(int size, IItemContainerStrategy strategy, ArrayPool<ItemStack>? pool = null)
		{
			arrayPool = pool ?? ArrayPool<ItemStack>.Shared;
			this.strategy = strategy;
			slots = null!;
			SetSize(size);
		}

		public virtual bool TrySetSize(int value)
		{
			using ItemContainerTransaction transaction = ItemContainerTransaction.Begin();

			if (!strategy.TrySetSize(ref transaction.GetContext(this), value))
			{
				return false;
			}

			transaction.Commit();
			return true;
		}

		public virtual bool TryAdd(in ItemStack stack, out ItemStack remainder)
		{
			using ItemContainerTransaction transaction = ItemContainerTransaction.Begin();

			if (!strategy.TryAdd(ref transaction.GetContext(this), stack, out remainder))
			{
				return false;
			}

			transaction.Commit();
			return true;
		}

		public virtual bool TryAdd(int index, in ItemStack stack, out ItemStack result, out ItemStack remainder)
		{
			using ItemContainerTransaction transaction = ItemContainerTransaction.Begin();

			if (!strategy.TryAdd(ref transaction.GetContext(this), index, stack, out result, out remainder))
			{
				return false;
			}

			transaction.Commit();
			return true;
		}

		public virtual bool TryRemove(in ItemStack stack)
		{
			using ItemContainerTransaction transaction = ItemContainerTransaction.Begin();

			if (!strategy.TryRemove(ref transaction.GetContext(this), stack))
			{
				return false;
			}

			transaction.Commit();
			return true;
		}

		public virtual bool TryRemove(int index, int count, out ItemStack result, out ItemStack removed)
		{
			using ItemContainerTransaction transaction = ItemContainerTransaction.Begin();

			if (!strategy.TryRemove(ref transaction.GetContext(this), index, count, out result, out removed))
			{
				return false;
			}

			transaction.Commit();
			return true;
		}

		public virtual bool TryMove(int fromIndex, int count, IItemContainer to)
		{
			using ItemContainerTransaction t = ItemContainerTransaction.Begin();

			if (!strategy.TryRemove(ref t.GetContext(this), fromIndex, count, out _, out ItemStack removed))
			{
				return false;
			}

			if (!to.Strategy.TryAdd(ref t.GetContext(to), removed, out ItemStack remainder))
			{
				return false;
			}

			if (remainder != default && (!strategy.TryAdd(ref t.GetContext(this), fromIndex, remainder, out _, out ItemStack rem) || rem != default))
			{
				return false;
			}

			t.Commit();
			return true;
		}

		public virtual bool TryMove(int fromIndex, int count, IItemContainer to, int toIndex)
		{
			using ItemContainerTransaction t = ItemContainerTransaction.Begin();

			ref ItemContainerTransactionContext fromCtx = ref t.GetContext(this);

			if (!strategy.TryRemove(ref fromCtx, fromIndex, count, out ItemStack result, out ItemStack fromSlot))
			{
				return false;
			}

			ref ItemContainerTransactionContext toCtx = ref t.GetContext(to);

			if (to.Strategy.TryAdd(ref toCtx, toIndex, fromSlot, out _, out ItemStack remainder))
			{
				if (remainder == default || strategy.TryAdd(ref fromCtx, fromIndex, remainder, out _, out remainder) && remainder == default)
				{
					t.Commit();
					return true;
				}
			}
			else if (result == default && to.Strategy.TryRemove(ref toCtx, toIndex, -1, out _, out ItemStack toSlot))
			{
				if (!strategy.TryAdd(ref fromCtx, fromIndex, toSlot, out _, out remainder) || remainder != default)
				{
					return false;
				}

				if (!to.Strategy.TryAdd(ref toCtx, toIndex, fromSlot, out _, out remainder) || remainder != default)
				{
					return false;
				}

				t.Commit();
				return true;
			}

			return false;
		}

		public virtual void CopyFrom(ReadOnlySpan<ItemStack> stacks)
		{
			using ItemContainerTransaction transaction = ItemContainerTransaction.Begin();
			ref ItemContainerTransactionContext ctx = ref transaction.GetContext(this);
			int count = stacks.Length;

			if (size < count)
			{
				ctx.SetSize(count);
			}

			for (int i = 0; i < count; i++)
			{
				ItemStack slot = slots[i];
				ItemStack stack = stacks[i];

				if (slot != stack)
				{
					ctx.SetSlot(i, stack);
				}
			}

			for (int i = count; i < size; i++)
			{
				ItemStack slot = slots[i];

				if (slot != default)
				{
					ctx.SetSlot(i, default);
				}
			}

			transaction.Commit();
		}

		public virtual void CopyFrom(IReadOnlyList<ItemStack> stacks)
		{
			using ItemContainerTransaction transaction = ItemContainerTransaction.Begin();
			ref ItemContainerTransactionContext ctx = ref transaction.GetContext(this);
			int count = stacks.Count;

			if (size < count)
			{
				ctx.SetSize(count);
			}

			for (int i = 0; i < count; i++)
			{
				ItemStack slot = slots[i];
				ItemStack stack = stacks[i];

				if (slot != stack)
				{
					ctx.SetSlot(i, stack);
				}
			}

			for (int i = count; i < size; i++)
			{
				ItemStack slot = slots[i];

				if (slot != default)
				{
					ctx.SetSlot(i, default);
				}
			}

			transaction.Commit();
		}

		public virtual void Clear()
		{
			using ItemContainerTransaction transaction = ItemContainerTransaction.Begin();
			ref ItemContainerTransactionContext ctx = ref transaction.GetContext(this);

			for (int i = 0; i < size; i++)
			{
				ItemStack slot = slots[i];

				if (slot != default)
				{
					ctx.SetSlot(i, default);
				}
			}

			transaction.Commit();
		}

		public virtual void Sort() => Sort(ValueComparer<ItemStack>.Default);

		public virtual void Sort<TComparer>(TComparer comparer) where TComparer : IComparer<ItemStack>
		{
			ItemStack[] original = arrayPool.Rent(size);
			Array.Copy(slots, 0, original, 0, size);

			// We DON'T use the default Array.Sort because it is unstable. I think a stable sorting algorithm is very valuable here.
			// Our version also tends to be faster in general, so that's nice.
			TimSortLite.Sort(slots.AsSpan(0, size), comparer);

			for (int i = 0; i < size; i++)
			{
				ItemStack slot = slots[i];
				ItemStack originalSlot = original[i];

				if (slot != originalSlot)
				{
					OnSlotChanged(i, slot, originalSlot);
				}
			}

			Array.Clear(original, 0, size);
			arrayPool.Return(original);
		}

		public virtual void Commit(ReadOnlySpan<SlotEdit> slotEdits, SizeEdit? sizeEdit)
		{
			// First pass: Apply edits
			if (sizeEdit.HasValue)
			{
				SetSize(sizeEdit.Value.NewSize);
			}

			for (int i = 0; i < slotEdits.Length; i++)
			{
				ref readonly SlotEdit edit = ref slotEdits[i];

				if (edit.Index < size)
				{
					slots[edit.Index] = edit.NewStack;

					if (edit.OldStack.Count <= 0 && edit.NewStack.Count > 0) occupiedSlots++;
					else if (edit.OldStack.Count > 0 && edit.NewStack.Count <= 0) occupiedSlots--;
				}
			}

			// Second pass: Notify
			if (sizeEdit.HasValue)
			{
				OnSizeChanged(sizeEdit.Value.OldSize, sizeEdit.Value.NewSize);
			}

			for (int i = 0; i < slotEdits.Length; i++)
			{
				ref readonly SlotEdit edit = ref slotEdits[i];

				if (edit.Index < size)
				{
					OnSlotChanged(edit.Index, edit.OldStack, edit.NewStack);
				}
			}
		}

		public virtual ArrayEnumerator<ItemStack> GetEnumerator() => new(slots, size);

		IEnumerator<ItemStack> IEnumerable<ItemStack>.GetEnumerator() => GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		protected virtual void SetSize(int value)
		{
			if (value < 0) value = 0;
			if (value == size && slots != null) return;

			if (slots == null || value > slots.Length)
			{
				ItemStack[]? oldSlots = slots;
				slots = value > 0 ? arrayPool.Rent(value) : Array.Empty<ItemStack>();

				// We SHOULD NOT return an empty array to the pool
				if (oldSlots != null && oldSlots.Length > 0)
				{
					Array.Copy(oldSlots, slots, size);
					Array.Clear(oldSlots, 0, size);
					arrayPool.Return(oldSlots);
				}
			}
			else if (value < size)
			{
				Array.Clear(slots, value, size);
			}

			size = value;
		}
	}
}