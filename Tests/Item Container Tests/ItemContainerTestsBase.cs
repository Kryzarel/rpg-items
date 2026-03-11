using System;
using System.Linq;
using NUnit.Framework;

namespace Kryz.RPG.Items.Tests
{
	public abstract class ItemContainerTestsBase<T> where T : IItemContainer
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
	}
}