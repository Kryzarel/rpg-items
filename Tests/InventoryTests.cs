using System;
using Kryz.RPG.Items.Classes;
using Kryz.RPG.Items.Interfaces;
using Kryz.RPG.Items.Structs;
using NUnit.Framework;
using UnityEngine;

namespace Kryz.RPG.Items.Tests
{
	public class InventoryTests
	{
		private class TestItemDefinition : IItemDefinition
		{
			public Sprite Icon => null!;
			public string Name => "Test Item";
			public string Description => "An item just for testing";
			public int MaxStacks => 20;
		}

		private class TestItem : IItem
		{
			public IItemDefinition Definition { get; private set; }

			public TestItem(IItemDefinition itemDefinition) => Definition = itemDefinition;
		}

		private const string sizesSouce = nameof(sizes);
		private const string itemsSouce = nameof(items);
		private const string stackSizesSouce = nameof(stackSizes);

		private static readonly int[] sizes = { -1, 0, 1, 10, 100 };
		private static readonly TestItemDefinition definition = new();
		private static readonly IItem[] items = { null!, new TestItem(definition), new TestItem(definition) };
		private static readonly int[] stackSizes = { -1, 0, 1, definition.MaxStacks / 2, definition.MaxStacks, definition.MaxStacks * 2 };

		[Test]
		public void Iventory_Create([ValueSource(sizesSouce)] int size)
		{
			Inventory inventory = new(size);

			int expectedSize = Math.Max(0, size);

			Assert.AreEqual(expectedSize, inventory.Count, 0);
			Assert.AreEqual(expectedSize, inventory.NumSlots, 0);
			Assert.AreEqual(expectedSize, inventory.FreeSlots, 0);
			Assert.AreEqual(0, inventory.FullSlots, 0);

			if (expectedSize <= 0)
			{
				Assert.Throws<IndexOutOfRangeException>(() => { ItemStack stack = inventory[0]; });
			}
			else
			{
				for (int i = 0; i < inventory.Count; i++)
				{
					ItemStack slot = inventory[i];
					Assert.IsTrue(slot.Item == null && slot.Count == 0);
				}
			}
		}

		[Test]
		public void Iventory_Add_OneItem([ValueSource(sizesSouce)] int size, [ValueSource(itemsSouce)] IItem item, [ValueSource(stackSizesSouce)] int stackSize)
		{
			Inventory inventory = new(size);
			ItemStack stack = new(item, stackSize);

			if (inventory.NumSlots == 0 || stack.Count == 0 || stack.Item == null)
			{
				Assert.IsFalse(inventory.TryAdd(stack, out ItemStack remainder));
				Assert.AreEqual(stack, remainder);
			}
			else
			{
				int freeSlots = inventory.FreeSlots;
				int requiredSlots = (int)Math.Ceiling(stack.Count / (float)stack.Item.Definition.MaxStacks);

				Assert.IsTrue(inventory.TryAdd(stack, out ItemStack remainder));

				if (requiredSlots <= freeSlots)
				{
					Assert.AreEqual(default(ItemStack), remainder);
				}
				else
				{
					Assert.AreNotEqual(default(ItemStack), remainder);
				}
			}
		}

		[Test]
		public void Iventory_Remove_OneItem([ValueSource(sizesSouce)] int size, [ValueSource(itemsSouce)] IItem item, [ValueSource(stackSizesSouce)] int stackSize)
		{
			Inventory inventory = new(size);
			ItemStack stack = new(item, stackSize);
			inventory.TryAdd(stack, out ItemStack remainder);

			if (inventory.NumSlots == 0 || stack.Count == 0 || stack.Item == null)
			{
				Assert.IsFalse(inventory.TryRemove(item, stackSize, out ItemStack removed));
				Assert.AreEqual(stack, remainder);
			}
			else
			{

			}
		}
	}
}