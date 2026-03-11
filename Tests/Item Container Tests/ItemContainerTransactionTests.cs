using System;
using NUnit.Framework;

namespace Kryz.RPG.Items.Tests
{
	public abstract class ItemContainerTransactionTests<T> : ItemContainerTestsBase<T> where T : IItemContainer
	{
		protected static ReadOnlySpan<ItemStack> Snapshot(IReadOnlyItemContainer container)
		{
			ReadOnlySpan<ItemStack> slots = container.Slots;
			ItemStack[] copy = new ItemStack[slots.Length];
			slots.CopyTo(copy);
			return copy;
		}

		protected static void AssertAreEqual(ReadOnlySpan<ItemStack> expected, ReadOnlySpan<ItemStack> actual)
		{
			Assert.AreEqual(expected.Length, actual.Length);

			for (int i = 0; i < expected.Length; i++)
			{
				Assert.AreEqual(expected[i], actual[i]);
			}
		}

		[Test]
		public virtual void Transaction_Commit_PersistsChanges()
		{
			T container = CreateContainer(4);

			using (ItemTransaction tx = new(container))
			{
				container.TryAdd(tx, new ItemStack(itemA, 5), out _);
				tx.SetResult(true);
			}

			Assert.AreEqual(5, container[0].Count);
			AssertInvariants(container);
		}

		[Test]
		public virtual void Transaction_Rollback_RevertsChanges()
		{
			T container = CreateContainer(4);

			ReadOnlySpan<ItemStack> before = Snapshot(container);

			using (ItemTransaction tx = new(container))
			{
				container.TryAdd(tx, new ItemStack(itemA, 5), out _);
			}

			AssertInvariants(container);
			AssertAreEqual(before, container.Slots);
		}

		[Test]
		public virtual void Transaction_MultipleOperationsCommit()
		{
			T container = CreateContainer(4);

			using (ItemTransaction tx = new(container))
			{
				container.TryAdd(tx, new ItemStack(itemA, 5), out _);
				container.TryAdd(tx, new ItemStack(itemB, 3), out _);

				tx.SetResult(true);
			}

			Assert.AreEqual(2, container.OccupiedSlots);
			AssertInvariants(container);
		}

		[Test]
		public virtual void Transaction_MultipleOperationsRollback()
		{
			T container = CreateContainer(4);

			using (ItemTransaction tx = new(container))
			{
				container.TryAdd(tx, new ItemStack(itemA, 5), out _);
				container.TryAdd(tx, new ItemStack(itemB, 3), out _);
			}

			Assert.AreEqual(0, container.OccupiedSlots);
			AssertInvariants(container);
		}

		[Test]
		public virtual void Transaction_EventFiresOnlyOnCommit()
		{
			T container = CreateContainer(4);

			int fired = 0;
			container.OnTransactionSuccess += () => fired++;

			using (ItemTransaction tx = new(container))
			{
				container.TryAdd(tx, new ItemStack(itemA, 5), out _);
				Assert.AreEqual(0, fired);
				tx.SetResult(true);
			}

			Assert.AreEqual(1, fired);
		}

		[Test]
		public virtual void Transaction_EventNotFiredOnRollback()
		{
			T container = CreateContainer(4);

			int fired = 0;
			container.OnTransactionSuccess += () => fired++;

			using (ItemTransaction tx = new(container))
			{
				container.TryAdd(tx, new ItemStack(itemA, 5), out _);
			}

			Assert.AreEqual(0, fired);
		}

		[Test]
		public virtual void Transaction_SlotEventsDelayedUntilCommit()
		{
			T container = CreateContainer(4);

			int fired = 0;
			container.OnSlotChanged += (_, _, _) => fired++;

			using (ItemTransaction tx = new(container))
			{
				container.TryAdd(tx, new ItemStack(itemA, 5), out _);
				Assert.AreEqual(0, fired);
				tx.SetResult(true);
			}

			Assert.Greater(fired, 0);
		}
	}
}