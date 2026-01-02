using System;
using System.Runtime.CompilerServices;

namespace Kryz.RPG.Items
{
	public readonly struct ItemStack : IComparable<ItemStack>, IEquatable<ItemStack>
	{
		public readonly IItem? Item;
		public readonly int Count;

		public ItemStack(IItem item) : this(item, 1) { }

		public ItemStack(IItem item, int count)
		{
			if (item != null && count > 0)
			{
				Item = item;
				Count = count;
			}
			else
			{
				Item = null;
				Count = 0;
			}
		}

		/// <summary>
		/// Try to add another item stack to this one.
		/// </summary>
		/// <param name="stack">The item stack to add.</param>
		/// <param name="remainder">If <paramref name="stack"/> could not be added in its entirety, this will contain how much remains of the original stack.</param>
		/// <param name="maxCount">The maximum amount of items the resulting stack can hold. Even though <see cref="IItem"/> already specifies <see cref="IItem.MaxStacks"/>, we may want to override that.</param>
		/// <returns>The <see cref="ItemStack"/> that resulted from merging the original stack with the '<paramref name="stack"/>' parameter.
		/// If the total count is larger than '<paramref name="maxCount"/>', check the '<paramref name="remainder"/>' variable for the items that couldn't be added.</returns>
		public ItemStack Add(in ItemStack stack, out ItemStack remainder, int maxCount = 0)
		{
			TryAdd(stack, out ItemStack result, out remainder, maxCount);
			return result;
		}

		/// <summary>
		/// Try to remove a certain number of items from this stack.
		/// </summary>
		/// <param name="count">The number of items to remove.</param>
		/// <param name="removedStack">The item stack that was removed, or an empty stack if it was not possible to remove the requested number of items.</param>
		/// <returns>The <see cref="ItemStack"/> that resulted from removing '<paramref name="count"/>' items. If it was not possible to remove '<paramref name="count"/>' items, the original stack will be returned instead.</returns>
		public ItemStack Remove(int count, out ItemStack removedStack)
		{
			TryRemove(count, out ItemStack result, out removedStack);
			return result;
		}

		/// <summary>
		/// Try to add another item stack to this one.
		/// </summary>
		/// <param name="stack">The item stack to add.</param>
		/// <param name="result">The <see cref="ItemStack"/> that resulted from merging the original stack with the '<paramref name="stack"/>' parameter.
		/// If the total count is larger than '<paramref name="maxCount"/>', check the '<paramref name="remainder"/>' variable for the items that couldn't be added.</param>
		/// <param name="remainder">If the <paramref name="stack"/> parameter could not be added in its entirety, this will contain how much remains of the original stack.</param>
		/// <param name="maxCount">The maximum amount of items the resulting stack can hold. Even though <see cref="IItem"/> already specifies <see cref="IItem.MaxStacks"/>, we may want to override that.</param>
		/// <returns>True if one or more items in the <paramref name="stack"/> parameter were added. False otherwise.</returns>
		public bool TryAdd(in ItemStack stack, out ItemStack result, out ItemStack remainder, int maxCount = 0)
		{
			if (stack.Item != null && (Item == null || stack.Item == Item))
			{
				if (maxCount <= 0)
				{
					maxCount = stack.Item.MaxStacks;
				}

				if (Count < maxCount)
				{
					int newCount = Count + stack.Count;
					int remainingCount = newCount - maxCount;
					int resultCount = remainingCount > 0 ? maxCount : newCount;

					result = new ItemStack(stack.Item, resultCount);
					remainder = new ItemStack(stack.Item, remainingCount);
					return true;
				}
			}

			result = this;
			remainder = stack;
			return false;
		}

		/// <summary>
		/// Try to remove a certain number of items from this stack.
		/// </summary>
		/// <param name="count">The number of items to remove.</param>
		/// <param name="removedStack">The item stack that was removed, or an empty stack if it was not possible to remove the requested number of items.</param>
		/// <param name="result">The <see cref="ItemStack"/> that resulted from removing '<paramref name="count"/>' items. If it was not possible to remove '<paramref name="count"/>' items, the original stack will be returned instead.</param>
		/// <returns>True if the original <see cref="ItemStack"/> had '<paramref name="count"/>' or more items. False otherwise.</returns>
		public bool TryRemove(int count, out ItemStack result, out ItemStack removedStack)
		{
			if (count > 0 && count <= Count && Item != null)
			{
				result = new ItemStack(Item, Count - count);
				removedStack = new ItemStack(Item, count);
				return true;
			}

			result = this;
			removedStack = default;
			return false;
		}

		public int CompareTo(ItemStack other)
		{
			if (Item == null) return other.Item == null ? 0 : 1;
			if (other.Item == null) return -1;

			int result = Item.CompareTo(other.Item);
			if (result == 0) result = Count.CompareTo(other.Count);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(ItemStack other)
		{
			return Item == other.Item && Count == other.Count;
		}

		public override bool Equals(object obj)
		{
			return obj is ItemStack other && Equals(other);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Item, Count);
		}

		public override string ToString()
		{
			return Item == null ? "Empty Stack" : $"{Item.Name} x{Count}";
		}

		public static bool operator ==(ItemStack a, ItemStack b) => a.Equals(b);
		public static bool operator !=(ItemStack a, ItemStack b) => !a.Equals(b);
	}
}