using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kryz.RPG.Items.Interfaces;
using Kryz.RPG.Items.Structs;
using Kryz.Utils;

namespace Kryz.RPG.Items.Classes
{
	public class Inventory : IInventory
	{
		private readonly ArrayPool<ItemStack> arrayPool;

		private ItemStack[] slots;
		private int numSlots;
		private int filledSlots;

		public event IInventory.OnSlotChangedDelegate OnSlotChanged = delegate { };
		public event IInventory.OnNumSlotsChangedDelegate OnNumSlotsChanged = delegate { };

		public ItemStack this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				if (index < 0 || index >= numSlots)
					throw new IndexOutOfRangeException();
				return slots[index];
			}
		}

		public int Count => numSlots;
		public int NumSlots { get => numSlots; set => SetNumSlots(value); }
		public int FreeSlots => numSlots - filledSlots;
		public int FilledSlots => filledSlots;

		public Inventory(int numSlots, ArrayPool<ItemStack>? arrayPool = null)
		{
			this.arrayPool = arrayPool ?? ArrayPool<ItemStack>.Shared;
			slots = null!;
			SetNumSlots(numSlots);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool CanSet(int index, in ItemStack stack)
		{
			return index >= 0 && index < numSlots && stack.Item != null && stack.Count <= stack.Item.MaxStacks;
		}

		public bool CanAdd(in ItemStack stack, out ItemStack remainder, bool allowPartial = false)
		{
			remainder = stack;
			if (stack.Item == null)
			{
				return false;
			}

			for (int i = 0; i < numSlots && remainder.Count > 0; i++)
			{
				// Call ItemStack.TryAdd() but don't assign the result to anything.
				// This way we check what WOULD happen without actually changing the inventory.
				slots[i].TryAdd(remainder, out _, out remainder);
			}

			if (remainder.Count < stack.Count && (remainder.Count == 0 || allowPartial))
			{
				return true;
			}

			remainder = stack;
			return false;
		}

		public bool CanAdd(int index, in ItemStack stack, out ItemStack remainder, bool allowPartial = false)
		{
			if (stack.Item == null || index < 0 || index >= numSlots)
			{
				remainder = stack;
				return false;
			}

			if (slots[index].TryAdd(stack, out _, out remainder) && (remainder.Count == 0 || allowPartial))
			{
				return true;
			}

			remainder = stack;
			return false;
		}

		public bool CanRemoveThenAdd(IReadOnlyList<ItemStack> remove, IReadOnlyList<ItemStack> add)
		{
			InventoryChanges changes = new(slots[..numSlots], stackalloc int[numSlots], arrayPool);

			bool success = TryRemoveThenAdd(remove, add, ref changes);

			changes.Dispose();
			return success;
		}

		public bool Contains(in ItemStack stack)
		{
			if (stack.Item == null)
			{
				return false;
			}

			for (int i = 0, count = stack.Count; i < numSlots; i++)
			{
				ItemStack slot = slots[i];

				if (slot.Item == stack.Item)
				{
					count -= slot.Count;

					if (count <= 0)
					{
						return true;
					}
				}
			}

			return false;
		}

		public bool TrySet(int index, in ItemStack stack, bool forceSet = false)
		{
			if (index < 0 || index >= numSlots)
			{
				return false;
			}

			ref ItemStack slot = ref slots[index];

			if (forceSet || slot != stack && CanSet(index, stack))
			{
				SetSlot(ref slot, index, stack);
				return true;
			}

			return false;
		}

		public bool TryAdd(in ItemStack stack, out ItemStack remainder, bool allowPartial = false)
		{
			remainder = stack;
			if (stack.Item == null)
			{
				return false;
			}

			InventoryChanges changes = new(slots[..numSlots], stackalloc int[numSlots], arrayPool);
			bool success = TryAdd(stack, out remainder, ref changes);

			if (success && (remainder.Count == 0 || allowPartial))
			{
				ApplyChanges(changes);
			}
			else
			{
				remainder = stack;
			}

			changes.Dispose();
			return success;
		}

		public bool TryAdd(int index, in ItemStack stack, out ItemStack remainder, bool allowPartial = false)
		{
			if (stack.Item == null || index < 0 || index >= numSlots)
			{
				remainder = stack;
				return false;
			}

			ref ItemStack slot = ref slots[index];

			if (slot.TryAdd(stack, out ItemStack result, out remainder) && (remainder.Count == 0 || allowPartial))
			{
				SetSlot(ref slot, index, result);
				return true;
			}

			remainder = stack;
			return false;
		}

		public bool TryRemove(in ItemStack stack)
		{
			if (stack.Item == null)
			{
				return false;
			}

			InventoryChanges changes = new(slots[..numSlots], stackalloc int[numSlots], arrayPool);
			bool success = TryRemove(stack, ref changes);

			if (success)
			{
				ApplyChanges(changes);
			}

			changes.Dispose();
			return success;
		}

		public bool TryRemove(int index, int count, out ItemStack removed)
		{
			if (index < 0 || index >= numSlots)
			{
				removed = default;
				return false;
			}
			return TryRemove(ref slots[index], index, count, out removed);
		}

		public bool TryRemove(int index, out ItemStack removed)
		{
			if (index < 0 || index >= numSlots)
			{
				removed = default;
				return false;
			}
			ref ItemStack slot = ref slots[index];
			return TryRemove(ref slot, index, slot.Count, out removed);
		}

		public bool TryRemoveThenAdd(IReadOnlyList<ItemStack> remove, IReadOnlyList<ItemStack> add)
		{
			InventoryChanges changes = new(slots[..numSlots], stackalloc int[numSlots], arrayPool);

			bool success = TryRemoveThenAdd(remove, add, ref changes);

			if (success)
			{
				ApplyChanges(changes);
			}

			changes.Dispose();
			return success;
		}

		public bool TryMove(int toIndex, int fromIndex, IInventory fromInv, bool allowSwap = false)
		{
			if (toIndex < 0 || toIndex >= numSlots || fromIndex < 0 || fromIndex >= fromInv.NumSlots || (toIndex == fromIndex && this == fromInv))
			{
				return false;
			}

			ItemStack fromSlot = fromInv[fromIndex];

			if (fromSlot.Item == null)
			{
				return false;
			}

			ref ItemStack toSlot = ref slots[toIndex];

			if (toSlot.TryAdd(fromSlot, out ItemStack result, out ItemStack remainder) && fromInv.TrySet(fromIndex, remainder))
			{
				SetSlot(ref toSlot, toIndex, result);
				return true;
			}
			else if (allowSwap && CanSet(toIndex, fromSlot) && fromInv.TrySet(fromIndex, toSlot))
			{
				SetSlot(ref toSlot, toIndex, fromSlot);
				return true;
			}
			return false;
		}

		public void Sort(IComparer<ItemStack>? comparer = null)
		{
			ItemStack[] original = arrayPool.Rent(numSlots);
			Array.Copy(slots, 0, original, 0, numSlots);

			Array.Sort(slots, 0, numSlots, comparer ?? Comparer<ItemStack>.Default);

			for (int i = 0; i < numSlots; i++)
			{
				ItemStack slot = slots[i];
				ItemStack originalSlot = original[i];

				if (slot != originalSlot)
				{
					OnSlotChanged(i, slot, originalSlot);
				}
			}

			Array.Clear(original, 0, numSlots);
			arrayPool.Return(original);
		}

		public void Clear()
		{
			for (int i = 0; i < numSlots; i++)
			{
				ref ItemStack slot = ref slots[i];

				if (slot != default)
				{
					SetSlot(ref slot, i, default);
				}
			}
		}

		public ArrayEnumerator<ItemStack> GetEnumerator() => new(slots, numSlots);

		IEnumerator<ItemStack> IEnumerable<ItemStack>.GetEnumerator() => GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void SetSlot(ref ItemStack slot, int index, in ItemStack stack)
		{
			if (slot.Count <= 0 && stack.Count > 0) filledSlots++;
			else if (slot.Count > 0 && stack.Count <= 0) filledSlots--;
			ItemStack previous = slot;
			slot = stack;
			OnSlotChanged(index, slot, previous);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool TryRemove(ref ItemStack slot, int index, int count, out ItemStack removed)
		{
			if (slot.TryRemove(count, out ItemStack result, out removed))
			{
				SetSlot(ref slot, index, result);
				return true;
			}
			return false;
		}

		private bool TryRemove(ItemStack stack, ref InventoryChanges changes)
		{
			int required = stack.Count;

			for (int i = 0; i < numSlots && required > 0; i++)
			{
				ref ItemStack slot = ref slots[i];

				if (slot.Item == stack.Item && slot.TryRemove(Math.Min(required, slot.Count), out ItemStack result, out ItemStack removed))
				{
					required -= removed.Count;
					changes.StoreChange(i, result);
				}
			}

			return required <= 0;
		}

		private bool TryAdd(in ItemStack stack, out ItemStack remainder, ref InventoryChanges changes)
		{
			remainder = stack;

			if (stack.Item == null)
			{
				return false;
			}

			int maxCount = stack.Item.MaxStacks;

			// First, try adding to slots that already have the same item
			for (int i = 0; i < numSlots && remainder.Count > 0; i++)
			{
				ref ItemStack slot = ref slots[i];

				if (slot.Item == stack.Item && slot.TryAdd(remainder, out ItemStack result, out remainder, maxCount))
				{
					changes.StoreChange(i, result);
				}
			}

			// Then, try adding to empty slots
			for (int i = 0; i < numSlots && remainder.Count > 0; i++)
			{
				ref ItemStack slot = ref slots[i];

				if (slot.Item == null && slot.TryAdd(remainder, out ItemStack result, out remainder, maxCount))
				{
					changes.StoreChange(i, result);
				}
			}

			return remainder.Count < stack.Count; // Return true if we added at least 1 item
		}

		private bool TryRemoveThenAdd(IReadOnlyList<ItemStack> remove, IReadOnlyList<ItemStack> add, ref InventoryChanges changes)
		{
			for (int i = 0; i < remove.Count; i++)
			{
				if (!TryRemove(remove[i], ref changes))
				{
					return false;
				}
			}

			for (int i = 0; i < add.Count; i++)
			{
				if (!TryAdd(add[i], out ItemStack remainder, ref changes) || remainder.Count > 0)
				{
					return false;
				}
			}

			return true;
		}

		private void ApplyChanges(InventoryChanges changes)
		{
			foreach (int i in changes.Indexes)
			{
				ref ItemStack slot = ref slots[i];
				ItemStack newSlot = changes.NewSlots[i];

				if (slot != newSlot)
				{
					SetSlot(ref slot, i, newSlot);
				}
			}
		}

		private void SetNumSlots(int value)
		{
			if (numSlots == value && slots != null)
			{
				return;
			}

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
			else if (slots == null || value > slots.Length)
			{
				ItemStack[] newSlots = arrayPool.Rent(value);

				if (slots != null && slots.Length > 0)
				{
					Array.Copy(slots, newSlots, numSlots);
					Array.Clear(slots, 0, numSlots);
					arrayPool.Return(slots);
				}

				slots = newSlots;
			}

			int previous = numSlots;
			numSlots = value;
			OnNumSlotsChanged(numSlots, previous);
		}
	}
}