// using System;
// using System.Buffers;
// using System.Collections;
// using System.Collections.Generic;
// using Kryz.Utils;

// namespace Kryz.RPG.Items
// {
// 	public abstract class BaseItemContainer : IItemContainer
// 	{
// 		protected readonly ArrayPool<ItemStack> arrayPool;

// 		protected ItemStack[] slots;
// 		protected int size;
// 		protected int occupiedSlots;

// 		public ItemStack this[int index]
// 		{
// 			get
// 			{
// 				if ((uint)index >= (uint)size)
// 					throw new IndexOutOfRangeException("Index was out of range. Must be non-negative and less than the size of the collection.");
// 				return slots[index];
// 			}
// 		}

// 		public ReadOnlySpan<ItemStack> Slots => slots.AsSpan(0, size);

// 		public int Count => size;
// 		public int Size { get => size; set => SetSize(value); }
// 		public int FreeSlots => size - occupiedSlots;
// 		public int OccupiedSlots => occupiedSlots;

// 		public event IItemContainer.OnSlotChangedDelegate OnSlotChanged = delegate { };
// 		public event IItemContainer.OnSizeChangedDelegate OnSizeChanged = delegate { };

// 		public BaseItemContainer(int size, ArrayPool<ItemStack>? pool = null)
// 		{
// 			arrayPool = pool ?? ArrayPool<ItemStack>.Shared;
// 			slots = null!;
// 			SetSize(size);
// 		}

// 		public virtual int GetMaxStacks(IItem item) => item.MaxStacks;

// 		public virtual bool CanReceive(int index, in ItemStack stack) => true;

// 		public virtual bool CanRemove(int index) => true;

// 		public virtual bool TrySetSize(int value)
// 		{
// 			using ItemContainerTransaction transaction = new(this);

// 			if (!TrySetSize(transaction, value))
// 			{
// 				return false;
// 			}

// 			transaction.Commit();
// 			return true;
// 		}

// 		public virtual bool TryAdd(in ItemStack stack, out ItemStack remainder)
// 		{
// 			using ItemContainerTransaction transaction = new(this);

// 			if (!TryAdd(transaction, stack, out remainder))
// 			{
// 				return false;
// 			}

// 			transaction.Commit();
// 			return true;
// 		}

// 		public virtual bool TryAdd(int index, in ItemStack stack, out ItemStack result, out ItemStack remainder)
// 		{
// 			using ItemContainerTransaction transaction = new(this);

// 			if (!TryAdd(transaction, index, stack, out result, out remainder))
// 			{
// 				return false;
// 			}

// 			transaction.Commit();
// 			return true;
// 		}

// 		public virtual bool TryRemove(in ItemStack stack)
// 		{
// 			using ItemContainerTransaction transaction = new(this);

// 			if (!TryRemove(transaction, stack))
// 			{
// 				return false;
// 			}

// 			transaction.Commit();
// 			return true;
// 		}

// 		public virtual bool TryRemove(int index, int count, out ItemStack result, out ItemStack removed)
// 		{
// 			using ItemContainerTransaction transaction = new(this);

// 			if (!TryRemove(transaction, index, count, out result, out removed))
// 			{
// 				return false;
// 			}

// 			transaction.Commit();
// 			return true;
// 		}

// 		public virtual bool TryMove(int fromIndex, int count, IItemContainer to)
// 		{
// 			using ItemContainerTransaction txFrom = new(this);
// 			using ItemContainerTransaction txTo = new(to);

// 			if (!TryRemove(txFrom, fromIndex, count, out _, out ItemStack removed))
// 			{
// 				return false;
// 			}

// 			if (!to.TryAdd(txTo, removed, out ItemStack remainder))
// 			{
// 				return false;
// 			}

// 			if (!remainder.IsEmpty && (!TryAdd(txFrom, fromIndex, remainder, out _, out ItemStack rem) || !rem.IsEmpty))
// 			{
// 				return false;
// 			}

// 			txFrom.Commit();
// 			txTo.Commit();
// 			return true;
// 		}

// 		public virtual bool TryMove(int fromIndex, int count, IItemContainer to, int toIndex)
// 		{
// 			using ItemContainerTransaction txFrom = new(this);
// 			using ItemContainerTransaction txTo = new(to);

// 			if (!TryRemove(txFrom, fromIndex, count, out ItemStack result, out ItemStack fromSlot))
// 			{
// 				return false;
// 			}

// 			if (to.TryAdd(txTo, toIndex, fromSlot, out _, out ItemStack remainder))
// 			{
// 				if (remainder.IsEmpty || TryAdd(txFrom, fromIndex, remainder, out _, out remainder) && remainder.IsEmpty)
// 				{
// 					txFrom.Commit();
// 					txTo.Commit();
// 					return true;
// 				}
// 			}
// 			else if (result.IsEmpty && to.TryRemove(txTo, toIndex, -1, out _, out ItemStack toSlot))
// 			{
// 				if (!TryAdd(txFrom, fromIndex, toSlot, out _, out remainder) || !remainder.IsEmpty)
// 				{
// 					return false;
// 				}

// 				if (!to.TryAdd(txTo, toIndex, fromSlot, out _, out remainder) || !remainder.IsEmpty)
// 				{
// 					return false;
// 				}

// 				txFrom.Commit();
// 				txTo.Commit();
// 				return true;
// 			}

// 			return false;
// 		}

// 		public virtual void CopyFrom(ReadOnlySpan<ItemStack> stacks)
// 		{
// 			using ItemContainerTransaction transaction = new(this);
// 			int count = stacks.Length;

// 			if (size < count)
// 			{
// 				transaction.SetSize(count);
// 			}

// 			for (int i = 0; i < count; i++)
// 			{
// 				ItemStack slot = slots[i];
// 				ItemStack stack = stacks[i];

// 				if (slot != stack)
// 				{
// 					transaction.SetSlot(i, stack);
// 				}
// 			}

// 			for (int i = count; i < size; i++)
// 			{
// 				ItemStack slot = slots[i];

// 				if (!slot.IsEmpty)
// 				{
// 					transaction.SetSlot(i, default);
// 				}
// 			}

// 			transaction.Commit();
// 		}

// 		public virtual void CopyFrom(IReadOnlyList<ItemStack> stacks)
// 		{
// 			using ItemContainerTransaction transaction = new(this);
// 			int count = stacks.Count;

// 			if (size < count)
// 			{
// 				transaction.SetSize(count);
// 			}

// 			for (int i = 0; i < count; i++)
// 			{
// 				ItemStack slot = slots[i];
// 				ItemStack stack = stacks[i];

// 				if (slot != stack)
// 				{
// 					transaction.SetSlot(i, stack);
// 				}
// 			}

// 			for (int i = count; i < size; i++)
// 			{
// 				ItemStack slot = slots[i];

// 				if (!slot.IsEmpty)
// 				{
// 					transaction.SetSlot(i, default);
// 				}
// 			}

// 			transaction.Commit();
// 		}

// 		public virtual void Clear()
// 		{
// 			using ItemContainerTransaction transaction = new(this);

// 			for (int i = 0; i < size; i++)
// 			{
// 				ItemStack slot = slots[i];

// 				if (!slot.IsEmpty)
// 				{
// 					transaction.SetSlot(i, default);
// 				}
// 			}

// 			transaction.Commit();
// 		}

// 		public virtual void Sort() => Sort(ValueComparer<ItemStack>.Default);

// 		public virtual void Sort<TComparer>(TComparer comparer) where TComparer : IComparer<ItemStack>
// 		{
// 			ItemStack[] original = arrayPool.Rent(size);
// 			Array.Copy(slots, original, size);

// 			// We DON'T use the default Array.Sort because it is unstable. I think stable sorting is very valuable here.
// 			// Our version also tends to be just as fast, if not slightly faster, which is nice.
// 			TimSortLite.Sort(slots.AsSpan(0, size), comparer);

// 			for (int i = 0; i < size; i++)
// 			{
// 				ItemStack slot = slots[i];
// 				ItemStack originalSlot = original[i];

// 				if (slot != originalSlot)
// 				{
// 					OnSlotChanged(i, slot);
// 				}
// 			}

// 			Array.Clear(original, 0, size);
// 			arrayPool.Return(original);
// 		}

// 		public virtual void Commit(ReadOnlySpan<SlotEdit> slotEdits, int? sizeEdit)
// 		{
// 			// First pass: Apply edits
// 			if (sizeEdit.HasValue)
// 			{
// 				SetSize(sizeEdit.Value);
// 			}

// 			for (int i = 0; i < slotEdits.Length; i++)
// 			{
// 				ref readonly SlotEdit edit = ref slotEdits[i];

// 				if ((uint)edit.Index < (uint)size)
// 				{
// 					ref ItemStack slot = ref slots[edit.Index];

// 					if (slot.Count <= 0 && edit.NewStack.Count > 0) occupiedSlots++;
// 					else if (slot.Count > 0 && edit.NewStack.Count <= 0) occupiedSlots--;

// 					slot = edit.NewStack;
// 				}
// 			}

// 			// Second pass: Notify
// 			if (sizeEdit.HasValue)
// 			{
// 				OnSizeChanged(size);
// 			}

// 			for (int i = 0; i < slotEdits.Length; i++)
// 			{
// 				ref readonly SlotEdit edit = ref slotEdits[i];

// 				if ((uint)edit.Index < (uint)size)
// 				{
// 					OnSlotChanged(edit.Index, slots[edit.Index]);
// 				}
// 			}
// 		}

// 		public virtual bool TryAdd(ItemContainerTransaction transaction, in ItemStack stack, out ItemStack remainder)
// 		{
// 			remainder = stack;

// 			if (stack.Item == null)
// 			{
// 				return false;
// 			}

// 			int maxCount = GetMaxStacks(stack.Item);
// 			int size = transaction.GetSize();

// 			// First, try adding to slots that already have the same item
// 			for (int i = 0; i < size && remainder.Count > 0; i++)
// 			{
// 				ItemStack slot = transaction.GetSlot(i);

// 				if (slot.Item == stack.Item && CanReceive(i, stack) && slot.TryAdd(stack, out ItemStack result, out remainder, maxCount))
// 				{
// 					transaction.SetSlot(i, result);
// 				}
// 			}

// 			// Then, try adding to empty slots
// 			for (int i = 0; i < size && remainder.Count > 0; i++)
// 			{
// 				ItemStack slot = transaction.GetSlot(i);

// 				if (slot.Item == null && CanReceive(i, stack) && slot.TryAdd(stack, out ItemStack result, out remainder, maxCount))
// 				{
// 					transaction.SetSlot(i, result);
// 				}
// 			}

// 			return remainder.Count < stack.Count; // Return true if we added at least 1 item
// 		}

// 		public virtual bool TryAdd(ItemContainerTransaction transaction, int index, in ItemStack stack, out ItemStack result, out ItemStack remainder)
// 		{
// 			ItemStack slot = transaction.GetSlot(index);

// 			if (stack.Item != null && CanReceive(index, stack) && slot.TryAdd(stack, out result, out remainder, GetMaxStacks(stack.Item)))
// 			{
// 				transaction.SetSlot(index, result);
// 				return true;
// 			}

// 			remainder = stack;
// 			result = slot;
// 			return false;
// 		}

// 		public virtual bool TryRemove(ItemContainerTransaction transaction, in ItemStack stack)
// 		{
// 			if (stack.Item == null)
// 			{
// 				return false;
// 			}

// 			int size = transaction.GetSize();

// 			for (int i = 0, required = stack.Count; i < size; i++)
// 			{
// 				ItemStack slot = transaction.GetSlot(i);

// 				if (slot.Item == stack.Item && CanRemove(i) && slot.TryRemove(Math.Min(required, slot.Count), out ItemStack result, out ItemStack removed))
// 				{
// 					transaction.SetSlot(i, result);
// 					required -= removed.Count;

// 					if (required <= 0)
// 					{
// 						return true;
// 					}
// 				}
// 			}

// 			return false;
// 		}

// 		public virtual bool TryRemove(ItemContainerTransaction transaction, int index, int count, out ItemStack result, out ItemStack removed)
// 		{
// 			ItemStack slot = transaction.GetSlot(index);

// 			if (count < 0) count = slot.Count;

// 			if (CanRemove(index) && slot.TryRemove(count, out result, out removed))
// 			{
// 				transaction.SetSlot(index, result);
// 				return true;
// 			}

// 			removed = default;
// 			result = slot;
// 			return false;
// 		}

// 		public virtual bool TrySetSize(ItemContainerTransaction transaction, int value)
// 		{
// 			int size = transaction.GetSize();

// 			if (value == size)
// 			{
// 				return false;
// 			}

// 			// If we're shrinking the container
// 			if (value < size)
// 			{
// 				for (int i = value; i < size; i++)
// 				{
// 					// But there are items in the slots that would be removed
// 					if (!transaction.GetSlot(i).IsEmpty)
// 					{
// 						// Don't shrink
// 						return false;
// 					}
// 				}
// 			}

// 			transaction.SetSize(value);
// 			return true;
// 		}

// 		public virtual ArrayEnumerator<ItemStack> GetEnumerator() => new(slots, size);

// 		IEnumerator<ItemStack> IEnumerable<ItemStack>.GetEnumerator() => GetEnumerator();
// 		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

// 		protected virtual void SetSize(int value)
// 		{
// 			if (value < 0) value = 0;
// 			if (value == size && slots != null) return;

// 			if (slots == null || value > slots.Length)
// 			{
// 				ItemStack[]? oldSlots = slots;
// 				slots = value > 0 ? arrayPool.Rent(value) : Array.Empty<ItemStack>();

// 				// We SHOULD NOT return an empty array to the pool
// 				if (oldSlots != null && oldSlots.Length > 0)
// 				{
// 					Array.Copy(oldSlots, slots, size);
// 					Array.Clear(oldSlots, 0, size);
// 					arrayPool.Return(oldSlots);
// 				}
// 			}
// 			else if (value < size)
// 			{
// 				Array.Clear(slots, value, size);
// 			}

// 			size = value;
// 		}
// 	}
// }