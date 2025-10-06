using System;
using System.Runtime.CompilerServices;
using Kryz.RPG.Items.Interfaces;

namespace Kryz.RPG.Items.Structs
{
	public readonly struct ItemStack : IEquatable<ItemStack>
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

		public ItemStack Add(ItemStack toAdd, int maxCount, out ItemStack remainder)
		{
			TryAdd(toAdd, maxCount, out ItemStack stack, out remainder);
			return stack;
		}

		public ItemStack Remove(int count, out ItemStack removedStack)
		{
			TryRemove(count, out ItemStack stack, out removedStack);
			return stack;
		}

		public bool TryAdd(ItemStack toAdd, int maxCount, out ItemStack result, out ItemStack remainder)
		{
			if (toAdd.Count > 0 && toAdd.Item != null && (toAdd.Item == Item || Item == null))
			{
				int newCount = Count + toAdd.Count;
				int remainingCount = newCount - maxCount;
				int resultCount = remainingCount > 0 ? maxCount : newCount;

				result = new ItemStack(toAdd.Item, resultCount);
				remainder = new ItemStack(toAdd.Item, remainingCount);
				return true;
			}

			result = this;
			remainder = toAdd;
			return false;
		}

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
			return Item == null ? "Empty Stack" : $"{Item.Definition.Name}x{Count}";
		}

		public static bool operator ==(ItemStack a, ItemStack b) => a.Equals(b);
		public static bool operator !=(ItemStack a, ItemStack b) => !a.Equals(b);
	}
}