using System;
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

		private ItemStack RandomStack()
		{
			IItem item = rng.Next(2) == 0 ? itemA : itemB;
			int count = rng.Next(1, 40);
			return new ItemStack(item, count);
		}

		[Test]
		public void RandomOperationsMaintainInvariants()
		{
			T container = CreateContainer(10);

			for (int i = 0; i < 5000; i++)
			{
				int op = rng.Next(5);

				switch (op)
				{
					case 0:
						container.TryAdd(RandomStack(), out _);
						break;

					case 1:
						container.TryRemove(RandomStack());
						break;

					case 2:
						container.Sort();
						break;

					case 3:
						container.TrySetSize(rng.Next(0, 15));
						break;

					case 4:
						if (container.Size > 0)
						{
							int a = rng.Next(container.Size);
							int b = rng.Next(container.Size);
							container.TryMoveSlot(a, container, b);
						}
						break;
				}

				AssertInvariants(container);
			}
		}
	}
}