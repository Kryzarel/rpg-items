using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Kryz.RPG.Items.Tests
{
	public abstract class ItemContainerFuzzTests<T> : ItemContainerBehaviourTests<T> where T : IItemContainer
	{
		protected enum InventoryOperation
		{
			TryAdd,
			TryAddSlot,
			TryRemove,
			TryRemoveSlot,
			TryMove,
			TryMoveSlot,
			TrySetSize,
			Sort,
			Clear
		}

		protected static readonly int numOps = Enum.GetValues(typeof(InventoryOperation)).Length;

		protected virtual ItemStack RandomStack()
		{
			IItem item = rng.Next(2) == 0 ? itemA : itemB;
			int count = rng.Next(1, 40);
			return new ItemStack(item, count);
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