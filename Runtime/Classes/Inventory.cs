using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using Kryz.RPG.Items.Interfaces;
using Kryz.RPG.Items.Structs;

namespace Kryz.RPG.Items.Classes
{
	public class Inventory : IInventory
	{
		private readonly ArrayPool<ItemStack> arrayPool;

		private ItemStack[] slots;
		private int numSlots;
		private int fullSlots;

		public int NumSlots { get => numSlots; set => SetNumSlots(value); }
		public int FullSlots => fullSlots;
		public int FreeSlots => numSlots - fullSlots;

		public ItemStack this[int index] => slots[index];

		int IReadOnlyCollection<ItemStack>.Count => numSlots;

		public Inventory(int numSlots, ArrayPool<ItemStack>? arrayPool = null)
		{
			this.arrayPool = arrayPool ?? ArrayPool<ItemStack>.Shared;
			this.numSlots = numSlots;
			slots = this.numSlots > 0 ? this.arrayPool.Rent(numSlots) : Array.Empty<ItemStack>();
			fullSlots = 0;
		}

		public bool TryAdd(ItemStack stack, out ItemStack remainder)
		{
			if (stack.Item == null || stack.Count == 0)
			{
				remainder = stack;
				return false;
			}

			int startingCount = stack.Count;
			// We might have some modifier to the maximum stacks each inventory can hold
			int maxCount = stack.Item.Definition.MaxStacks;
			// If the stack has more items than 1 free slot can take, we need multiple free slots
			// Fast integer division that always rounds up: https://stackoverflow.com/a/4175152
			int freeSlotsNeeded = (stack.Count + maxCount - 1) / maxCount;

			// First we try to add to slots that already have this item
			ReadOnlySpan<int> freeSlots = AddOrGetFreeSlots(ref stack, maxCount, stackalloc int[freeSlotsNeeded]);

			// Success, we added the whole stack
			if (stack.Count == 0)
			{
				remainder = default;
				return true;
			}

			// If we still haven't added the whole stack, we try to add to free slots
			foreach (int index in freeSlots)
			{
				// Get the slot by reference so we can change it without having to reindex into the array
				ref ItemStack slot = ref slots[index];

				bool wasEmpty = slot.Count == 0;
				slot = slot.Add(stack, maxCount, out stack);
				bool isEmpty = slot.Count == 0;

				if (wasEmpty && !isEmpty)
				{
					fullSlots++;
				}

				// Success, we added the whole stack
				if (stack.Item == null || stack.Count == 0)
				{
					remainder = default;
					return true;
				}
			}

			remainder = stack;
			return remainder.Count < startingCount; // Return true if we added at least 1 item
		}

		public bool TryAdd(int index, ItemStack stack, out ItemStack remainder)
		{
			if (index < 0 || index >= numSlots || stack.Item == null || stack.Count == 0)
			{
				remainder = stack;
				return false;
			}

			int maxCount = stack.Item.Definition.MaxStacks;
			ref ItemStack slot = ref slots[index];

			bool wasEmpty = slot.Count == 0;

			if (slot.TryAdd(stack, maxCount, out ItemStack result, out stack))
			{
				slot = result;
				remainder = stack;

				bool isEmpty = slot.Count == 0;
				if (wasEmpty && !isEmpty)
				{
					fullSlots++;
				}
				return true;
			}

			remainder = stack;
			return false;
		}

		public bool TryMove(int from, int to)
		{
			if (from < 0 || from >= numSlots || to < 0 || to >= numSlots)
			{
				return false;
			}

			ref ItemStack fromSlot = ref slots[from];
			if (fromSlot.Item == null || fromSlot.Count == 0)
			{
				return false;
			}

			ref ItemStack toSlot = ref slots[to];
			int maxCount = fromSlot.Item.Definition.MaxStacks;

			if (toSlot.TryAdd(fromSlot, maxCount, out ItemStack result, out ItemStack remainder))
			{
				fromSlot = remainder;
				toSlot = result;
				return true;
			}

			(fromSlot, toSlot) = (toSlot, fromSlot);
			return true;
		}

		public bool TryRemove(int index, out ItemStack stack)
		{
			if (index < 0 || index >= numSlots)
			{
				stack = default;
				return false;
			}

			ref ItemStack slot = ref slots[index];
			if (slot.Item == null || slot.Count == 0)
			{
				stack = default;
				return false;
			}

			stack = slot;
			slot = default;
			fullSlots--;
			return true;
		}

		public Enumerator<ItemStack> GetEnumerator() => new(slots, numSlots);

		IEnumerator<ItemStack> IEnumerable<ItemStack>.GetEnumerator() => GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		private ReadOnlySpan<int> AddOrGetFreeSlots(ref ItemStack stack, int maxCount, Span<int> freeBuffer)
		{
			int freeCount = 0;

			for (int i = 0; i < numSlots; i++)
			{
				// Get the slot by reference so we can change it without having to reindex into the array
				ref ItemStack slot = ref slots[i];

				if (slot.Item == stack.Item)
				{
					bool wasEmpty = slot.Count == 0;
					slot = slot.Add(stack, maxCount, out stack);
					bool isEmpty = slot.Count == 0;

					if (wasEmpty && !isEmpty)
					{
						fullSlots++;
					}

					// Success, we added the whole stack
					if (stack.Item == null || stack.Count == 0)
					{
						return default;
					}
				}
				else if (freeCount < freeBuffer.Length && slot.Item == null)
				{
					freeBuffer[freeCount++] = i;
				}
			}

			return freeBuffer[..freeCount];
		}

		private void SetNumSlots(int value)
		{
			if (value <= 0)
			{
				value = 0;

				if (slots != null)
				{
					Array.Clear(slots, 0, numSlots);
					arrayPool.Return(slots);
				}

				slots = Array.Empty<ItemStack>();
			}
			else if (value > slots.Length)
			{
				ItemStack[] newSlots = arrayPool.Rent(value);

				if (slots != null)
				{
					Array.Copy(slots, newSlots, numSlots);
					Array.Clear(slots, 0, numSlots);
					arrayPool.Return(slots);
				}

				slots = newSlots;
			}

			numSlots = value;
		}

		public struct Enumerator<T> : IEnumerable<T>, IEnumerator<T>
		{
			private readonly T[] array;
			private readonly int count;

			private int index;
			private T current;

			public readonly T Current => current;

			readonly object IEnumerator.Current => Current!;

			public Enumerator(T[] array) : this(array, array.Length) { }

			public Enumerator(T[] array, int count)
			{
				this.array = array;
				this.count = count;

				index = 0;
				current = default!;
			}

			public bool MoveNext()
			{
				if (index < count)
				{
					current = array[index++];
					return true;
				}
				return false;
			}

			public void Reset()
			{
				index = 0;
				current = default!;
			}

			public readonly void Dispose() { }

			readonly IEnumerator<T> IEnumerable<T>.GetEnumerator() => this;
			readonly IEnumerator IEnumerable.GetEnumerator() => this;
		}
	}
}