using System;
using NUnit.Framework;

namespace Kryz.RPG.Items.Tests
{
	public abstract class ItemContainerCanTryTests<T> : ItemContainerTransactionTests<T> where T : IItemContainer
	{
		[Test]
		public virtual void CanAdd_Matches_TryAdd()
		{
			T container = CreateContainer(4);

			ItemStack stack = new(itemA, 7);

			ReadOnlySpan<ItemStack> before = Snapshot(container);

			bool can = container.CanAdd(stack, out ItemStack canRem);

			AssertAreEqual(before, container.Slots);

			bool result = container.TryAdd(stack, out ItemStack tryRem);

			Assert.AreEqual(can, result);
			Assert.AreEqual(canRem, tryRem);
			AssertInvariants(container);
		}

		[Test]
		public virtual void CanAddSlot_Matches_TryAddSlot()
		{
			T container = CreateContainer(4);

			int slot = 1;
			ItemStack stack = new(itemA, 10);

			ReadOnlySpan<ItemStack> before = Snapshot(container);

			bool can = container.CanAddSlot(slot, stack, out ItemStack canRem, out ItemStack canResult);

			AssertAreEqual(before, container.Slots);

			bool result = container.TryAddSlot(slot, stack, out ItemStack tryRem, out ItemStack tryResult);

			Assert.AreEqual(can, result);
			Assert.AreEqual(canRem, tryRem);
			Assert.AreEqual(canResult, tryResult);
			AssertInvariants(container);
		}

		[Test]
		public virtual void CanRemove_Matches_TryRemove()
		{
			T container = CreateContainer(4);

			container.TryAdd(new ItemStack(itemA, 10), out _);

			ItemStack stack = new(itemA, 5);

			ReadOnlySpan<ItemStack> before = Snapshot(container);

			bool can = container.CanRemove(stack);

			AssertAreEqual(before, container.Slots);

			bool result = container.TryRemove(stack);

			Assert.AreEqual(can, result);
			AssertInvariants(container);
		}

		[Test]
		public virtual void CanRemoveSlot_Matches_TryRemoveSlot()
		{
			T container = CreateContainer(4);

			container.TryAdd(new ItemStack(itemA, 10), out _);

			ReadOnlySpan<ItemStack> before = Snapshot(container);

			bool can = container.CanRemoveSlot(0, out ItemStack canResult, out ItemStack canRemoved, 4);

			AssertAreEqual(before, container.Slots);

			bool result = container.TryRemoveSlot(0, out ItemStack tryResult, out ItemStack tryRemoved, 4);

			Assert.AreEqual(can, result);
			Assert.AreEqual(canResult, tryResult);
			Assert.AreEqual(canRemoved, tryRemoved);
			AssertInvariants(container);
		}

		[Test]
		public virtual void CanMove_Matches_TryMove()
		{
			T a = CreateContainer(4);
			T b = CreateContainer(4);

			a.TryAdd(new ItemStack(itemA, 10), out _);

			ReadOnlySpan<ItemStack> snapA = Snapshot(a);
			ReadOnlySpan<ItemStack> snapB = Snapshot(b);

			bool can = a.CanMove(0, b, 4);

			AssertAreEqual(snapA, a.Slots);
			AssertAreEqual(snapB, b.Slots);

			bool result = a.TryMove(0, b, 4);

			Assert.AreEqual(can, result);
			AssertInvariants(a);
			AssertInvariants(b);
		}

		[Test]
		public virtual void CanMoveSlot_Matches_TryMoveSlot()
		{
			T a = CreateContainer(4);
			T b = CreateContainer(4);

			a.TryAdd(new ItemStack(itemA, 5), out _);

			ReadOnlySpan<ItemStack> snapA = Snapshot(a);
			ReadOnlySpan<ItemStack> snapB = Snapshot(b);

			bool can = a.CanMoveSlot(0, b, 1);

			AssertAreEqual(snapA, a.Slots);
			AssertAreEqual(snapB, b.Slots);

			bool result = a.TryMoveSlot(0, b, 1);

			Assert.AreEqual(can, result);
			AssertInvariants(a);
			AssertInvariants(b);
		}

		[Test]
		public virtual void CanSetSize_Matches_TrySetSize()
		{
			T container = CreateContainer(4);

			ReadOnlySpan<ItemStack> before = Snapshot(container);

			bool can = container.CanSetSize(2);

			AssertAreEqual(before, container.Slots);

			bool result = container.TrySetSize(2);

			Assert.AreEqual(can, result);
			AssertInvariants(container);
		}
	}
}