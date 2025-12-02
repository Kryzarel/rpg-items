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
		private class TestItem : IItem
		{
			public Sprite Icon => null!;
			public string Name => "Test Item";
			public string Description => "An item just for testing";
			public int MaxStacks => maxStacks;

			public int CompareTo(IItem other) => Name.CompareTo(other.Name);
		}

		private const int maxStacks = 20;

		private const string inventorySizesSrc = nameof(inventorySizes);
		private const string itemsSrc = nameof(items);
		private const string stackSizesSrc = nameof(stackSizes);

		private static readonly int[] inventorySizes = { -1, 0, 1, 10, 100 };
		private static readonly IItem[] items = { null!, new TestItem(), new TestItem() };
		private static readonly int[] stackSizes = { 1, maxStacks / 2, maxStacks, maxStacks * 2 };

		[Test]
		public void Create([ValueSource(inventorySizesSrc)] int size)
		{
			Inventory inventory = new(size);

			int expectedSize = Math.Max(0, size);

			Assert.AreEqual(expectedSize, inventory.Count, 0);
			Assert.AreEqual(expectedSize, inventory.Size, 0);
			Assert.AreEqual(expectedSize, inventory.FreeSlots, 0);
			Assert.AreEqual(0, inventory.OccupiedSlots, 0);

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
		public void Add_OneStack_Full([ValueSource(inventorySizesSrc)] int size, [ValueSource(itemsSrc)] IItem item, [ValueSource(stackSizesSrc)] int stackSize)
		{
			Inventory inventory = new(size);
			ItemStack stack = new(item, stackSize);

			if (inventory.Size == 0 || stack.Count == 0 || stack.Item == null)
			{
				Assert.IsFalse(inventory.CanAdd(stack, out _, allowPartial: false));
				Assert.IsFalse(inventory.TryAdd(stack, out ItemStack remainder, allowPartial: false));
				Assert.AreEqual(stack, remainder);
			}
			else
			{
				int freeSlots = inventory.FreeSlots;
				int requiredSlots = (int)Math.Ceiling(stack.Count / (float)stack.Item.MaxStacks);
				bool canAdd = requiredSlots <= freeSlots;

				Assert.AreEqual(canAdd, inventory.CanAdd(stack, out _, allowPartial: false));

				bool success = inventory.TryAdd(stack, out ItemStack remainder, allowPartial: false);

				if (canAdd)
				{
					Assert.IsTrue(success);
					Assert.AreEqual(default(ItemStack), remainder);
				}
				else
				{
					Assert.IsFalse(success);
					Assert.AreEqual(stack, remainder);
				}

				Assert.AreEqual(canAdd ? freeSlots - requiredSlots : freeSlots, inventory.FreeSlots, 0);
				Assert.AreEqual(canAdd ? requiredSlots : 0, inventory.OccupiedSlots, 0);
			}
		}

		[Test]
		public void Add_OneStack_Partial([ValueSource(inventorySizesSrc)] int size, [ValueSource(itemsSrc)] IItem item, [ValueSource(stackSizesSrc)] int stackSize)
		{
			Inventory inventory = new(size);
			ItemStack stack = new(item, stackSize);

			if (inventory.Size == 0 || stack.Count == 0 || stack.Item == null)
			{
				Assert.IsFalse(inventory.CanAdd(stack, out _, allowPartial: true));
				Assert.IsFalse(inventory.TryAdd(stack, out ItemStack remainder, allowPartial: true));
				Assert.AreEqual(stack, remainder);
			}
			else
			{
				int freeSlots = inventory.FreeSlots;
				int requiredSlots = (int)Math.Ceiling(stack.Count / (float)stack.Item.MaxStacks);

				Assert.IsTrue(inventory.CanAdd(stack, out _, allowPartial: true));
				Assert.IsTrue(inventory.TryAdd(stack, out ItemStack remainder, allowPartial: true));

				if (requiredSlots <= freeSlots)
				{
					Assert.AreEqual(default(ItemStack), remainder);
				}
				else
				{
					Assert.AreNotEqual(default(ItemStack), remainder);
				}

				Assert.AreEqual(Math.Max(0, freeSlots - requiredSlots), inventory.FreeSlots, 0);
				Assert.AreEqual(Math.Min(inventory.Size, requiredSlots), inventory.OccupiedSlots, 0);
			}
		}

		[Test]
		public void Remove_OneStack([ValueSource(inventorySizesSrc)] int size, [ValueSource(itemsSrc)] IItem item, [ValueSource(stackSizesSrc)] int stackSize)
		{
			Inventory inventory = new(size);
			ItemStack stack = new(item, stackSize);
			inventory.TryAdd(stack, out ItemStack remainder, allowPartial: true);

			if (inventory.Size == 0 || stack.Count == 0 || stack.Item == null)
			{
				Assert.IsFalse(inventory.TryRemove(stack));
			}
			else if (remainder.Count <= 0)
			{
				Assert.IsTrue(inventory.TryRemove(stack));
			}
			else
			{
				Assert.IsFalse(inventory.TryRemove(stack));
			}
		}

		[Test]
		public void Add_TwoStacks_SameItem([ValueSource(stackSizesSrc)] int stackSize1, [ValueSource(stackSizesSrc)] int stackSize2)
		{
			Inventory inventory = new(2);
			ItemStack stack1 = new(items[1], stackSize1);
			ItemStack stack2 = new(items[1], stackSize2);

			Assert.IsTrue(inventory.TryAdd(stack1, out ItemStack remainder1, allowPartial: true));
			Assert.AreEqual(default(ItemStack), remainder1); // The inventory should always have space to add the entire first item stack

			if (inventory.FreeSlots > 0)
			{
				int freeSpaceForItem = 0;
				for (int i = 0; i < inventory.Size; i++)
				{
					ItemStack slot = inventory[i];
					if (slot.Item == null || slot.Item == stack2.Item)
					{
						freeSpaceForItem += stack2.Item!.MaxStacks - slot.Count;
					}
				}

				Assert.IsTrue(inventory.TryAdd(stack2, out ItemStack remainder2, allowPartial: true));
				Assert.AreEqual(Math.Max(0, stack2.Count - freeSpaceForItem), remainder2.Count, 0);
			}
			else
			{
				Assert.IsFalse(inventory.TryAdd(stack2, out ItemStack remainder2, allowPartial: true));
				Assert.AreEqual(stack2, remainder2);
			}

			Assert.IsTrue(inventory[0].Item == items[1]);

			if (stack1.Count + stack2.Count > stack1.Item!.MaxStacks)
			{
				Assert.IsTrue(inventory[1].Item == items[1]);
			}
			else
			{
				Assert.IsTrue(inventory[1].Item == null);
			}
		}

		[Test]
		public void Add_TwoStacks_DifferentItems([ValueSource(stackSizesSrc)] int stackSize1, [ValueSource(stackSizesSrc)] int stackSize2)
		{
			Inventory inventory = new(2);
			ItemStack stack1 = new(items[1], stackSize1);
			ItemStack stack2 = new(items[2], stackSize2);

			Assert.IsTrue(inventory.TryAdd(stack1, out ItemStack remainder1, allowPartial: true));
			Assert.AreEqual(default(ItemStack), remainder1); // The inventory should always have space to add the entire first item stack

			if (inventory.FreeSlots > 0)
			{
				int freeSpaceForItem = 0;
				for (int i = 0; i < inventory.Size; i++)
				{
					ItemStack slot = inventory[i];
					if (slot.Item == null || slot.Item == stack2.Item)
					{
						freeSpaceForItem += stack2.Item!.MaxStacks - slot.Count;
					}
				}

				Assert.IsTrue(inventory.TryAdd(stack2, out ItemStack remainder2, allowPartial: true));
				Assert.AreEqual(Math.Max(0, stack2.Count - freeSpaceForItem), remainder2.Count, 0);
			}
			else
			{
				Assert.IsFalse(inventory.TryAdd(stack2, out ItemStack remainder2, allowPartial: true));
				Assert.AreEqual(stack2, remainder2);
			}

			Assert.IsTrue(inventory[0].Item == items[1]);

			if (stack1.Count > stack1.Item!.MaxStacks)
			{
				Assert.IsTrue(inventory[1].Item == items[1]);
			}
			else
			{
				Assert.IsTrue(inventory[1].Item == items[2]);
			}
		}
	}
}