using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Kryz.RPG.Items.Tests
{
	public abstract class ItemContainerTests<T> where T : IItemContainer
	{
		protected readonly IItem itemA = new TestItem("A", 20);
		protected readonly IItem itemB = new TestItem("B", 20);
		protected readonly Random rng = new(12345);

		protected virtual T CreateContainer(int size) => (T)Activator.CreateInstance(typeof(T), size, null);

		protected virtual void AssertInvariants(T container)
		{
			int occupied = container.Count(s => !s.IsEmpty);
			int free = container.Count(s => s.IsEmpty);

			Assert.AreEqual(occupied, container.OccupiedSlots);
			Assert.AreEqual(free, container.FreeSlots);
			Assert.AreEqual(occupied + free, container.Size);

			for (int i = 0; i < container.Count; i++)
			{
				ItemStack slot = container[i];

				if (slot.Item != null)
				{
					Assert.Greater(slot.Count, 0);
					Assert.LessOrEqual(slot.Count, slot.Item.MaxStacks);
				}
				else
				{
					Assert.AreEqual(0, slot.Count);
				}
			}
		}

		[Test]
		public virtual void Add_ToEmptyContainer()
		{
			T container = CreateContainer(4);

			container.TryAdd(new ItemStack(itemA, 5), out ItemStack remainder);

			Assert.True(remainder.IsEmpty);
			Assert.AreEqual(5, container[0].Count);
			Assert.AreEqual(1, container.OccupiedSlots);
			AssertInvariants(container);
		}

		[Test]
		public virtual void Add_MergesExistingStacksFirst()
		{
			T container = CreateContainer(4);

			container.TryAdd(new ItemStack(itemA, 5), out _);
			container.TryAdd(new ItemStack(itemA, 3), out _);

			Assert.AreEqual(8, container[0].Count);
			Assert.AreEqual(1, container.OccupiedSlots);
			AssertInvariants(container);
		}

		[Test]
		public virtual void Add_UsesEmptySlotsWhenStackFull()
		{
			T container = CreateContainer(4);

			container.TryAdd(new ItemStack(itemA, 20), out _);
			container.TryAdd(new ItemStack(itemA, 5), out _);

			Assert.AreEqual(20, container[0].Count);
			Assert.AreEqual(5, container[1].Count);
			Assert.AreEqual(2, container.OccupiedSlots);
			AssertInvariants(container);
		}

		[Test]
		public virtual void Remove_FromSlot()
		{
			T container = CreateContainer(2);

			container.TryAdd(new ItemStack(itemA, 10), out _);
			container.TryRemoveSlot(0, out ItemStack result, out ItemStack removed, 4);

			Assert.AreEqual(6, result.Count);
			Assert.AreEqual(4, removed.Count);
			Assert.AreEqual(1, container.OccupiedSlots);
			AssertInvariants(container);
		}

		[Test]
		public virtual void Remove_UpdatesOccupiedSlots()
		{
			T container = CreateContainer(2);

			container.TryAdd(new ItemStack(itemA, 3), out _);
			container.TryRemoveSlot(0, out _, out _, 3);

			Assert.AreEqual(0, container.OccupiedSlots);
			AssertInvariants(container);
		}

		[Test]
		public virtual void Move_BetweenContainers()
		{
			T a = CreateContainer(4);
			T b = CreateContainer(4);

			a.TryAdd(new ItemStack(itemA, 6), out _);

			a.TryMove(0, b, 3);

			Assert.AreEqual(3, a[0].Count);
			Assert.AreEqual(3, b[0].Count);
			AssertInvariants(a);
			AssertInvariants(b);
		}

		[Test]
		public virtual void MoveSlot_SwapsWhenTargetOccupied()
		{
			T a = CreateContainer(2);
			T b = CreateContainer(2);

			a.TryAddSlot(0, new ItemStack(itemA, 5), out _, out _);
			b.TryAddSlot(0, new ItemStack(itemB, 2), out _, out _);

			a.TryMoveSlot(0, b, 0, -1);

			Assert.AreEqual(itemB, a[0].Item);
			Assert.AreEqual(itemA, b[0].Item);
			AssertInvariants(a);
			AssertInvariants(b);
		}

		[Test]
		public virtual void Resize_Fails_WhenItemsWouldBeLost()
		{
			T container = CreateContainer(4);

			container.TryAddSlot(3, new ItemStack(itemA, 1), out _, out _);

			Assert.False(container.TrySetSize(2));
			AssertInvariants(container);
		}

		[Test]
		public virtual void Resize_Succeeds_WhenSlotsEmpty()
		{
			T container = CreateContainer(4);

			Assert.True(container.TrySetSize(2));
			Assert.AreEqual(2, container.Size);
			AssertInvariants(container);
		}

		[Test]
		public virtual void Sort_OrdersByItemThenCount()
		{
			T container = CreateContainer(4);

			container.TryAddSlot(0, new ItemStack(itemB, 1), out _, out _);
			container.TryAddSlot(1, new ItemStack(itemA, 5), out _, out _);
			container.TryAddSlot(2, new ItemStack(itemA, 2), out _, out _);

			container.Sort();

			Assert.AreEqual(itemA, container[0].Item);
			Assert.AreEqual(2, container[0].Count);

			Assert.AreEqual(itemA, container[1].Item);
			Assert.AreEqual(5, container[1].Count);
			AssertInvariants(container);
		}

		[Test]
		public virtual void Transaction_CommitsChanges()
		{
			T container = CreateContainer(4);

			using (var tx = new ItemTransaction(container))
			{
				container.TryAdd(tx, new ItemStack(itemA, 5), out _);
				tx.SetResult(true);
			}

			Assert.AreEqual(5, container[0].Count);
			AssertInvariants(container);
		}

		[Test]
		public virtual void Transaction_RollbackRevertsChanges()
		{
			T container = CreateContainer(4);

			using (var tx = new ItemTransaction(container))
			{
				container.TryAdd(tx, new ItemStack(itemA, 5), out _);
			}

			Assert.True(container[0].IsEmpty);
			AssertInvariants(container);
		}

		[Test]
		public virtual void TransactionSuccess_SlotEventFiresOnCommit()
		{
			T container = CreateContainer(2);

			int fired = 0;
			container.OnSlotChanged += (_, _, _) => fired++;

			container.TryAdd(new ItemStack(itemA, 1), out _);

			Assert.AreEqual(1, fired);
			AssertInvariants(container);
		}

		[Test]
		public virtual void TransactionSuccess_EventFires()
		{
			T container = CreateContainer(2);

			int fired = 0;
			container.OnTransactionSuccess += () => fired++;

			container.TryAdd(new ItemStack(itemA, 1), out _);

			Assert.AreEqual(1, fired);
			AssertInvariants(container);
		}

		protected virtual ItemStack RandomStack()
		{
			IItem item = rng.Next(2) == 0 ? itemA : itemB;
			int count = rng.Next(1, 40);
			return new ItemStack(item, count);
		}

		protected static Dictionary<IItem, int> CountItems(params IReadOnlyItemContainer[] containers)
		{
			Dictionary<IItem, int> counts = new();

			foreach (IReadOnlyItemContainer container in containers)
			{
				for (int i = 0; i < container.Count; i++)
				{
					ItemStack slot = container[i];

					if (slot.Item == null) continue;

					counts.TryGetValue(slot.Item, out int current);
					counts[slot.Item] = current + slot.Count;
				}
			}

			return counts;
		}

		protected static int RandomOccupiedSlot(IReadOnlyItemContainer container, Random rng)
		{
			if (container.OccupiedSlots == 0)
				return -1;

			int target = rng.Next(container.OccupiedSlots);
			int count = 0;

			for (int i = 0; i < container.Size; i++)
			{
				if (container[i].IsEmpty)
					continue;
				if (count == target)
					return i;
				count++;
			}

			return -1;
		}

		protected static int RandomSlotPreferOccupied(IReadOnlyItemContainer container, Random rng)
		{
			int occupiedSlot = RandomOccupiedSlot(container, rng);
			return occupiedSlot >= 0 && rng.NextDouble() < 0.7 ? occupiedSlot : rng.Next(container.Size);
		}

		protected enum InventoryOperation { TryAdd, TryAddSlot, TryRemove, TryRemoveSlot, TryMove, TryMoveSlot, TrySetSize, Sort, Clear }

		protected static readonly int numOps = Enum.GetValues(typeof(InventoryOperation)).Length;

		[Test]
		public virtual void RandomOperationsMaintainInvariants()
		{
			T container = CreateContainer(10);

			for (int i = 0; i < 5000; i++)
			{
				InventoryOperation op = (InventoryOperation)rng.Next(numOps);

				switch (op)
				{
					case InventoryOperation.TryAdd:
						container.TryAdd(RandomStack(), out _);
						break;

					case InventoryOperation.TryAddSlot:
						if (container.Size > 0)
						{
							int index = RandomSlotPreferOccupied(container, rng);
							container.TryAddSlot(index, RandomStack(), out _, out _);
						}
						break;

					case InventoryOperation.TryRemove:
						container.TryRemove(RandomStack());
						break;

					case InventoryOperation.TryRemoveSlot:
						if (container.Size > 0)
						{
							int index = RandomSlotPreferOccupied(container, rng);
							int count = rng.Next(-1, 40);
							container.TryRemoveSlot(index, out _, out _, count);
						}
						break;

					case InventoryOperation.TryMove:
						if (container.Size > 0)
						{
							int index = RandomSlotPreferOccupied(container, rng);
							int count = rng.Next(-1, 40);
							container.TryMove(index, container, count);
						}
						break;

					case InventoryOperation.TryMoveSlot:
						if (container.Size > 0)
						{
							int index1 = RandomSlotPreferOccupied(container, rng);
							int index2 = RandomSlotPreferOccupied(container, rng);
							container.TryMoveSlot(index1, container, index2);
						}
						break;

					case InventoryOperation.TrySetSize:
						container.TrySetSize(rng.Next(0, 15));
						break;

					case InventoryOperation.Sort:
						container.Sort();
						break;

					case InventoryOperation.Clear:
						container.Clear();
						break;
				}

				AssertInvariants(container);
			}
		}

		[Test]
		public virtual void RandomOperationsMaintainInvariants_TwoInventories()
		{
			T a = CreateContainer(10);
			T b = CreateContainer(10);

			for (int i = 0; i < 5000; i++)
			{
				int c = rng.Next(2);
				T container = c == 0 ? a : b;
				T other = c == 0 ? b : a;

				Dictionary<IItem, int> before = CountItems(a, b);

				InventoryOperation op = (InventoryOperation)rng.Next(numOps);

				switch (op)
				{
					case InventoryOperation.TryAdd:
						container.TryAdd(RandomStack(), out _);
						break;

					case InventoryOperation.TryAddSlot:
						if (container.Size > 0)
						{
							int index = RandomSlotPreferOccupied(container, rng);
							container.TryAddSlot(index, RandomStack(), out _, out _);
						}
						break;

					case InventoryOperation.TryRemove:
						container.TryRemove(RandomStack());
						break;

					case InventoryOperation.TryRemoveSlot:
						if (container.Size > 0)
						{
							int index = RandomSlotPreferOccupied(container, rng);
							int count = rng.Next(-1, 40);
							container.TryRemoveSlot(index, out _, out _, count);
						}
						break;

					case InventoryOperation.TryMove:
						if (container.Size > 0)
						{
							int index = RandomSlotPreferOccupied(container, rng);
							int count = rng.Next(-1, 40);
							container.TryMove(index, other, count);
						}
						break;

					case InventoryOperation.TryMoveSlot:
						if (container.Size > 0 && other.Size > 0)
						{
							int index1 = RandomSlotPreferOccupied(container, rng);
							int index2 = RandomSlotPreferOccupied(other, rng);
							container.TryMoveSlot(index1, other, index2);
						}
						break;

					case InventoryOperation.TrySetSize:
						container.TrySetSize(rng.Next(0, 15));
						break;

					case InventoryOperation.Sort:
						container.Sort();
						break;

					case InventoryOperation.Clear:
						container.Clear();
						break;
				}

				AssertInvariants(a);
				AssertInvariants(b);

				// Adds and removes are allowed to change totals
				if (op is InventoryOperation.TryAdd or InventoryOperation.TryAddSlot or InventoryOperation.TryRemove
					or InventoryOperation.TryRemoveSlot or InventoryOperation.Clear or InventoryOperation.TrySetSize)
					continue;

				Dictionary<IItem, int> after = CountItems(a, b);

				foreach (IItem item in before.Keys.Union(after.Keys))
				{
					before.TryGetValue(item, out int beforeCount);
					after.TryGetValue(item, out int afterCount);
					Assert.AreEqual(beforeCount, afterCount, $"Item count changed for {item.Name}");
				}
			}
		}
	}
}