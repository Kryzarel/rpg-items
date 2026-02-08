using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using Kryz.Utils;

namespace Kryz.RPG.Items
{
	public abstract class ItemContainer : ItemContainerTransactional, IItemContainer
	{
		int IReadOnlyCollection<ItemStack>.Count => size;

		public ItemContainer(int size, ArrayPool<ItemStack>? pool = null) : base(size, pool)
		{
		}

		public bool TrySetSize(int value)
		{
			using ItemTransaction transaction = new(this);
			if (!TrySetSize(transaction, value))
			{
				return false;
			}
			transaction.Commit();
			return true;
		}

		public bool TryAdd(in ItemStack stack, out ItemStack remainder)
		{
			using ItemTransaction transaction = new(this);
			if (!TryAdd(transaction, stack, out remainder))
			{
				return false;
			}
			transaction.Commit();
			return true;
		}

		public bool TryAdd(int index, in ItemStack stack, out ItemStack result, out ItemStack remainder)
		{
			using ItemTransaction transaction = new(this);
			if (!TryAdd(transaction, index, stack, out result, out remainder))
			{
				return false;
			}
			transaction.Commit();
			return true;
		}

		public bool TryRemove(in ItemStack stack)
		{
			using ItemTransaction transaction = new(this);
			if (!TryRemove(transaction, stack))
			{
				return false;
			}
			transaction.Commit();
			return true;
		}

		public bool TryRemove(int index, int count, out ItemStack result, out ItemStack removed)
		{
			using ItemTransaction transaction = new(this);
			if (!TryRemove(transaction, index, count, out result, out removed))
			{
				return false;
			}
			transaction.Commit();
			return true;
		}

		public bool TryMove(int fromIndex, int count, IItemContainer to)
		{
			using ItemTransaction transaction = new(this);

			if (!TryRemove(transaction, fromIndex, count, out _, out ItemStack removed))
			{
				return false;
			}

			if (!to.TryAdd(transaction, removed, out ItemStack remainder))
			{
				return false;
			}

			if (!remainder.IsEmpty && (!TryAdd(transaction, fromIndex, remainder, out _, out ItemStack rem) || !rem.IsEmpty))
			{
				return false;
			}

			transaction.Commit();
			return true;
		}

		public bool TryMove(int fromIndex, int count, IItemContainer to, int toIndex)
		{
			using ItemTransaction transaction = new(this);

			if (!TryRemove(transaction, fromIndex, count, out ItemStack result, out ItemStack fromSlot))
			{
				return false;
			}

			if (to.TryAdd(transaction, toIndex, fromSlot, out _, out ItemStack remainder))
			{
				if (remainder.IsEmpty || TryAdd(transaction, fromIndex, remainder, out _, out remainder) && remainder.IsEmpty)
				{
					transaction.Commit();
					return true;
				}
			}
			else if (result.IsEmpty && to.TryRemove(transaction, toIndex, -1, out _, out ItemStack toSlot))
			{
				if (!TryAdd(transaction, fromIndex, toSlot, out _, out remainder) || !remainder.IsEmpty)
				{
					return false;
				}

				if (!to.TryAdd(transaction, toIndex, fromSlot, out _, out remainder) || !remainder.IsEmpty)
				{
					return false;
				}

				transaction.Commit();
				return true;
			}

			return false;
		}

		public void CopyFrom(ReadOnlySpan<ItemStack> stacks)
		{
			using ItemTransaction transaction = new(this);
			int count = stacks.Length;

			if (size < count)
			{
				SetSize(transaction, count);
			}

			for (int i = 0; i < count; i++)
			{
				ItemStack stack = stacks[i];
				ref ItemStack slot = ref slots[i];

				if (slot != stack)
				{
					SetSlot(transaction, ref slot, i, stack);
				}
			}

			transaction.Commit();
		}

		public void CopyFrom(IReadOnlyList<ItemStack> stacks)
		{
			using ItemTransaction transaction = new(this);
			int count = stacks.Count;

			if (size < count)
			{
				SetSize(transaction, count);
			}

			for (int i = 0; i < count; i++)
			{
				ItemStack stack = stacks[i];
				ref ItemStack slot = ref slots[i];

				if (slot != stack)
				{
					SetSlot(transaction, ref slot, i, stack);
				}
			}

			transaction.Commit();
		}

		public void Clear()
		{
			using ItemTransaction transaction = new(this);

			for (int i = 0; i < size; i++)
			{
				ref ItemStack slot = ref slots[i];

				if (slot != default)
				{
					SetSlot(transaction, ref slot, i, default);
				}
			}

			transaction.Commit();
		}

		public void Sort() => Sort(ValueComparer<ItemStack>.Default);

		public void Sort<TComparer>(TComparer comparer) where TComparer : IComparer<ItemStack>
		{
			using ItemTransaction transaction = new(this);
			ItemStack[] original = arrayPool.Rent(size);
			Array.Copy(slots, original, size);

			// We DON'T use the default Array.Sort because it is unstable. I think stable sorting is very valuable here.
			// Our version also tends to be just as fast, if not slightly faster, which is nice.
			TimSortLite.Sort(slots.AsSpan(0, size), comparer);

			for (int i = 0; i < size; i++)
			{
				ItemStack slot = slots[i];
				ItemStack originalSlot = original[i];

				if (slot != originalSlot)
				{
					transaction.RecordSlot(this, i, slot);
				}
			}

			Array.Clear(original, 0, size);
			arrayPool.Return(original);
			transaction.Commit();
		}

		public ArraySegment<ItemStack>.Enumerator GetEnumerator() => new ArraySegment<ItemStack>(slots, 0, size).GetEnumerator();

		IEnumerator<ItemStack> IEnumerable<ItemStack>.GetEnumerator() => GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}