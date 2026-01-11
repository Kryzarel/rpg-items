using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Kryz.RPG.Items.Tests
{
	public class InventoryTests
	{
		private class TestItem : IItem
		{
			private readonly string name;

			public IItemDefinition Definition => null!;
			public Sprite Icon => null!;
			public string Name => name;
			public string Description => "An item just for testing";
			public int MaxStacks => maxStacks;

			public int CompareTo(IItem other) => Name.CompareTo(other.Name);
			public override string ToString() => Name;
			public TestItem(string name) => this.name = name;
		}

		private const int maxStacks = 20;

		private const string inventorySizesSrc = nameof(inventorySizes);
		private const string itemsSrc = nameof(items);
		private const string stackSizesSrc = nameof(stackSizes);

		private static readonly int[] inventorySizes = { -1, 0, 1, 10, 100 };
		private static readonly IItem[] items = { null!, new TestItem("Test Item 1"), new TestItem("Test Item 2") };
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
		public void SetSize([ValueSource(inventorySizesSrc)] int size)
		{
			Inventory inventory = new(0);

			inventory.Size = size;
			int expectedSize = Math.Max(0, size);

			Assert.AreEqual(expectedSize, inventory.Count, 0);
			Assert.AreEqual(expectedSize, inventory.Size, 0);
			Assert.AreEqual(expectedSize, inventory.FreeSlots, 0);
			Assert.AreEqual(0, inventory.OccupiedSlots, 0);
			Assert.AreEqual(inventory.Count(s => s == default), inventory.FreeSlots);
			Assert.AreEqual(inventory.Count(s => s != default), inventory.OccupiedSlots);

			if (expectedSize <= 0)
			{
				Assert.Throws<IndexOutOfRangeException>(() => { ItemStack stack = inventory[0]; });
			}
			else
			{
				for (int i = 0; i < inventory.Count; i++)
				{
					ItemStack slot = inventory[i];
					Assert.AreEqual(default(ItemStack), slot);
				}
			}
		}

		[Test]
		public void SetSize_PreserveItems([ValueSource(inventorySizesSrc)] int size)
		{
			// Arrange
			ItemStack[] itemStacks = new ItemStack[] { default, new(items[1]), new(items[2]) };
			Inventory inventory = new(itemStacks);

			// Act
			inventory.Size = size;

			// Assert
			int expectedSize = Math.Max(0, size);
			int expectedOccupied = itemStacks[..Math.Min(expectedSize, itemStacks.Length)].Count(s => s != default);
			int expectedFree = Math.Max(0, expectedSize - expectedOccupied);

			Assert.AreEqual(expectedSize, inventory.Count, 0);
			Assert.AreEqual(expectedSize, inventory.Size, 0);
			Assert.AreEqual(expectedFree, inventory.FreeSlots, 0);
			Assert.AreEqual(expectedOccupied, inventory.OccupiedSlots, 0);
			Assert.AreEqual(inventory.Count(s => s == default), inventory.FreeSlots);
			Assert.AreEqual(inventory.Count(s => s != default), inventory.OccupiedSlots);

			if (expectedSize <= 0)
			{
				Assert.Throws<IndexOutOfRangeException>(() => { ItemStack stack = inventory[0]; });
			}
			else
			{
				for (int i = 0; i < inventory.Count; i++)
				{
					ItemStack slot = inventory[i];
					Assert.AreEqual(i < itemStacks.Length ? itemStacks[i] : default, slot);
				}
			}
		}

		[Test]
		public void Set([ValueSource(itemsSrc)] IItem item, [ValueSource(stackSizesSrc)] int stackSize)
		{
			Inventory inventory = new(1);
			ItemStack stack = new(item, stackSize);

			Assert.IsFalse(inventory.CanSwap(-1, stack, out _));
			Assert.IsFalse(inventory.TrySwap(-1, stack, out _, forceSwap: false));
			Assert.IsFalse(inventory.TrySwap(-1, stack, out _, forceSwap: true));

			Assert.IsFalse(inventory.CanSwap(inventory.Size, stack, out _));
			Assert.IsFalse(inventory.TrySwap(inventory.Size, stack, out _, forceSwap: false));
			Assert.IsFalse(inventory.TrySwap(inventory.Size, stack, out _, forceSwap: true));

			Assert.AreEqual(1, inventory.FreeSlots, 0);
			Assert.AreEqual(0, inventory.OccupiedSlots, 0);
			Assert.AreEqual(inventory.Count(s => s == default), inventory.FreeSlots);
			Assert.AreEqual(inventory.Count(s => s != default), inventory.OccupiedSlots);

			if (stack.Item != default)
			{
				if (stack.Count <= stack.Item.MaxStacks)
				{
					Assert.IsTrue(inventory.CanSwap(0, stack, out _));
				}
				else
				{
					Assert.IsFalse(inventory.CanSwap(0, stack, out _));
				}

				Assert.AreEqual(1, inventory.FreeSlots, 0);
				Assert.AreEqual(0, inventory.OccupiedSlots, 0);
				Assert.AreEqual(inventory.Count(s => s == default), inventory.FreeSlots);
				Assert.AreEqual(inventory.Count(s => s != default), inventory.OccupiedSlots);

				if (stack.Count <= stack.Item.MaxStacks)
				{
					Assert.IsTrue(inventory.TrySwap(0, stack, out _, forceSwap: false));
					Assert.AreEqual(0, inventory.FreeSlots, 0);
					Assert.AreEqual(1, inventory.OccupiedSlots, 0);
					Assert.AreEqual(inventory.Count(s => s == default), inventory.FreeSlots);
					Assert.AreEqual(inventory.Count(s => s != default), inventory.OccupiedSlots);
				}
				else
				{
					Assert.IsFalse(inventory.TrySwap(0, stack, out _, forceSwap: false));
					Assert.AreEqual(1, inventory.FreeSlots, 0);
					Assert.AreEqual(0, inventory.OccupiedSlots, 0);
					Assert.AreEqual(inventory.Count(s => s == default), inventory.FreeSlots);
					Assert.AreEqual(inventory.Count(s => s != default), inventory.OccupiedSlots);
				}

				if (inventory[0] == stack)
				{
					Assert.IsFalse(inventory.TrySwap(0, stack, out _, forceSwap: true)); // Fails because the slot already equals the item being set
					Assert.IsTrue(inventory.TrySwap(0, default, out _, forceSwap: true)); // Succeeds because we're setting it back to the default item
					Assert.IsTrue(inventory.TrySwap(0, stack, out _, forceSwap: true)); // Succeeds because we're setting it to a different item again
				}
				else
				{
					Assert.IsTrue(inventory.TrySwap(0, stack, out _, forceSwap: true));
				}

				Assert.AreEqual(0, inventory.FreeSlots, 0);
				Assert.AreEqual(1, inventory.OccupiedSlots, 0);
			}
			else
			{
				Assert.IsTrue(inventory.CanSwap(0, stack, out _));
				Assert.AreEqual(1, inventory.FreeSlots, 0);
				Assert.AreEqual(0, inventory.OccupiedSlots, 0);
				Assert.AreEqual(inventory.Count(s => s == default), inventory.FreeSlots);
				Assert.AreEqual(inventory.Count(s => s != default), inventory.OccupiedSlots);

				Assert.IsFalse(inventory.TrySwap(0, stack, out _, forceSwap: false)); // Fails because the slot already equals the default item
				Assert.AreEqual(1, inventory.FreeSlots, 0);
				Assert.AreEqual(0, inventory.OccupiedSlots, 0);
				Assert.AreEqual(inventory.Count(s => s == default), inventory.FreeSlots);
				Assert.AreEqual(inventory.Count(s => s != default), inventory.OccupiedSlots);

				Assert.IsFalse(inventory.TrySwap(0, stack, out _, forceSwap: true)); // Fails because the slot already equals the default item
				Assert.AreEqual(1, inventory.FreeSlots, 0);
				Assert.AreEqual(0, inventory.OccupiedSlots, 0);
				Assert.AreEqual(inventory.Count(s => s == default), inventory.FreeSlots);
				Assert.AreEqual(inventory.Count(s => s != default), inventory.OccupiedSlots);
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
				Assert.AreEqual(inventory.Size, inventory.FreeSlots, 0);
				Assert.AreEqual(0, inventory.OccupiedSlots, 0);
				Assert.AreEqual(inventory.Count(s => s == default), inventory.FreeSlots);
				Assert.AreEqual(inventory.Count(s => s != default), inventory.OccupiedSlots);
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

					Assert.AreEqual(inventory.Size - requiredSlots, inventory.FreeSlots, 0);
					Assert.AreEqual(requiredSlots, inventory.OccupiedSlots, 0);
					Assert.AreEqual(inventory.Count(s => s == default), inventory.FreeSlots);
					Assert.AreEqual(inventory.Count(s => s != default), inventory.OccupiedSlots);
				}
				else
				{
					Assert.IsFalse(success);
					Assert.AreEqual(stack, remainder);

					Assert.AreEqual(inventory.Size, inventory.FreeSlots, 0);
					Assert.AreEqual(0, inventory.OccupiedSlots, 0);
					Assert.AreEqual(inventory.Count(s => s == default), inventory.FreeSlots);
					Assert.AreEqual(inventory.Count(s => s != default), inventory.OccupiedSlots);
				}
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
				Assert.AreEqual(inventory.Size, inventory.FreeSlots, 0);
				Assert.AreEqual(0, inventory.OccupiedSlots, 0);
				Assert.AreEqual(inventory.Count(s => s == default), inventory.FreeSlots);
				Assert.AreEqual(inventory.Count(s => s != default), inventory.OccupiedSlots);
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

				Assert.AreEqual(Math.Max(0, inventory.Size - requiredSlots), inventory.FreeSlots, 0);
				Assert.AreEqual(Math.Min(inventory.Size, requiredSlots), inventory.OccupiedSlots, 0);
				Assert.AreEqual(inventory.Count(s => s == default), inventory.FreeSlots);
				Assert.AreEqual(inventory.Count(s => s != default), inventory.OccupiedSlots);
			}
		}

		[Test]
		public void Add_TwoStacks_SameItem([ValueSource(stackSizesSrc)] int stackSize1, [ValueSource(stackSizesSrc)] int stackSize2)
		{
			Inventory inventory = new(2);
			ItemStack stack1 = new(items[1], stackSize1);
			ItemStack stack2 = new(items[1], stackSize2);
			int maxStacks = stack2.Item != null ? stack2.Item.MaxStacks : 0;

			Assert.IsTrue(inventory.TryAdd(stack1, out ItemStack remainder1, allowPartial: true));
			Assert.AreEqual(default(ItemStack), remainder1); // The inventory should always have space to add the entire first item stack

			if (stack1.Count <= maxStacks)
			{
				int freeSpaceForItem = 0;
				for (int i = 0; i < inventory.Size; i++)
				{
					ItemStack slot = inventory[i];
					if (slot.Item == null || slot.Item == stack2.Item)
					{
						freeSpaceForItem += maxStacks - slot.Count;
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

			int requiredSlots = (int)Math.Ceiling((stack1.Count + stack2.Count) / (float)maxStacks);
			Assert.AreEqual(Math.Max(0, inventory.Size - requiredSlots), inventory.FreeSlots);
			Assert.AreEqual(Math.Min(inventory.Size, requiredSlots), inventory.OccupiedSlots);
			Assert.AreEqual(inventory.Count(s => s == default), inventory.FreeSlots);
			Assert.AreEqual(inventory.Count(s => s != default), inventory.OccupiedSlots);

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
			int maxStacks = stack2.Item != null ? stack2.Item.MaxStacks : 0;

			Assert.IsTrue(inventory.TryAdd(stack1, out ItemStack remainder1, allowPartial: true));
			Assert.AreEqual(default(ItemStack), remainder1); // The inventory should always have space to add the entire first item stack

			if (stack1.Count <= maxStacks)
			{
				int freeSpaceForItem = 0;
				for (int i = 0; i < inventory.Size; i++)
				{
					ItemStack slot = inventory[i];
					if (slot.Item == null || slot.Item == stack2.Item)
					{
						freeSpaceForItem += maxStacks - slot.Count;
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

			Assert.AreEqual(0, inventory.FreeSlots);
			Assert.AreEqual(2, inventory.OccupiedSlots);
			Assert.AreEqual(inventory.Count(s => s == default), inventory.FreeSlots);
			Assert.AreEqual(inventory.Count(s => s != default), inventory.OccupiedSlots);

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

		[Test]
		public void Add_Slot([ValueSource(itemsSrc)] IItem item, [ValueSource(stackSizesSrc)] int stackSize, [Values(true, false)] bool allowPartial)
		{
			Inventory inventory = new(3);
			ItemStack stack = new(item, stackSize);

			for (int i = 0; i < inventory.Size; i++)
			{
				inventory.TrySwap(i, new ItemStack(items[i]), out _, forceSwap: true);
			}

			for (int i = 0; i < inventory.Size; i++)
			{
				ItemStack slot = inventory[i];

				if (stack.Item == null)
				{
					Assert.IsFalse(inventory.CanAdd(i, stack, out ItemStack resultCan, out ItemStack remainderCan, allowPartial));
					Assert.IsFalse(inventory.TryAdd(i, stack, out ItemStack resultTry, out ItemStack remainderTry, allowPartial));
					Assert.AreEqual(resultCan, resultTry);
					Assert.AreEqual(remainderCan, remainderTry);
					Assert.AreEqual(default(ItemStack), remainderTry);

					Assert.AreEqual(inventory.Count(s => s == default), inventory.FreeSlots);
					Assert.AreEqual(inventory.Count(s => s != default), inventory.OccupiedSlots);
				}
				else if (slot.Item == null || slot.Item == stack.Item)
				{
					int maxStacks = stack.Item.MaxStacks;
					bool canAddAtLeastOne = slot.Count < maxStacks;
					bool canAddFullStack = slot.Count + stack.Count <= maxStacks;

					if (allowPartial && canAddAtLeastOne || canAddFullStack)
					{
						Assert.IsTrue(inventory.CanAdd(i, stack, out ItemStack resultCan, out ItemStack remainderCan, allowPartial));
						Assert.IsTrue(inventory.TryAdd(i, stack, out ItemStack resultTry, out ItemStack remainderTry, allowPartial));
						Assert.AreEqual(resultCan, resultTry);
						Assert.AreEqual(remainderCan, remainderTry);
						Assert.AreEqual(new ItemStack(stack.Item, stack.Count - (maxStacks - slot.Count)), remainderTry);
					}
					else
					{
						Assert.IsFalse(inventory.CanAdd(i, stack, out ItemStack resultCan, out ItemStack remainderCan, allowPartial));
						Assert.IsFalse(inventory.TryAdd(i, stack, out ItemStack resultTry, out ItemStack remainderTry, allowPartial));
						Assert.AreEqual(resultCan, resultTry);
						Assert.AreEqual(remainderCan, remainderTry);
						Assert.AreEqual(stack, remainderTry);
					}

					Assert.AreEqual(inventory.Count(s => s == default), inventory.FreeSlots);
					Assert.AreEqual(inventory.Count(s => s != default), inventory.OccupiedSlots);
				}
			}
		}

		[Test]
		public void CanRemove([ValueSource(inventorySizesSrc)] int size, [ValueSource(itemsSrc)] IItem item1, [ValueSource(stackSizesSrc)] int stackSize1, [ValueSource(itemsSrc)] IItem item2, [ValueSource(stackSizesSrc)] int stackSize2)
		{
			Inventory inventory = new(size);
			ItemStack stack1 = new(item1, stackSize1);
			ItemStack stack2 = new(item2, stackSize2);

			inventory.TryAdd(stack1, out _);
			int requiredSlots = stack1.Item != null ? (int)Math.Ceiling(stack1.Count / (float)stack1.Item.MaxStacks) : 0;

			if (inventory.Size >= requiredSlots && stack1.Item != null && stack1.Item == stack2.Item && stack1.Count >= stack2.Count)
			{
				Assert.IsTrue(inventory.CanRemove(stack2));
			}
			else
			{
				Assert.IsFalse(inventory.CanRemove(stack2));
			}
		}

		[Test]
		public void Remove_OneStack([ValueSource(inventorySizesSrc)] int size, [ValueSource(itemsSrc)] IItem item, [ValueSource(stackSizesSrc)] int stackSize)
		{
			Inventory inventory = new(size);
			ItemStack stack = new(item, stackSize);
			inventory.TryAdd(stack, out ItemStack remainder, allowPartial: true);
			int requiredSlots = stack.Item != null ? (int)Math.Ceiling(stack.Count / (float)stack.Item.MaxStacks) : 0;

			if (inventory.Size == 0 || stack.Count == 0 || stack.Item == null)
			{
				Assert.IsFalse(inventory.CanRemove(stack));
				Assert.IsFalse(inventory.TryRemove(stack));
				Assert.AreEqual(Math.Max(0, size - requiredSlots), inventory.FreeSlots);
				Assert.AreEqual(Math.Max(0, Math.Min(size, requiredSlots)), inventory.OccupiedSlots);
			}
			else if (remainder.Count <= 0)
			{
				Assert.IsTrue(inventory.CanRemove(stack));
				Assert.IsTrue(inventory.TryRemove(stack));
				Assert.AreEqual(inventory.Size, inventory.FreeSlots);
				Assert.AreEqual(0, inventory.OccupiedSlots);
			}
			else
			{
				Assert.IsFalse(inventory.CanRemove(stack));
				Assert.IsFalse(inventory.TryRemove(stack));
				Assert.AreEqual(Math.Max(0, size - requiredSlots), inventory.FreeSlots);
				Assert.AreEqual(Math.Max(0, Math.Min(size, requiredSlots)), inventory.OccupiedSlots);
			}

			Assert.AreEqual(inventory.Count(s => s == default), inventory.FreeSlots);
			Assert.AreEqual(inventory.Count(s => s != default), inventory.OccupiedSlots);
		}

		[Test]
		public void Remove_Slot([ValueSource(stackSizesSrc)] int stackSize)
		{
			Inventory inventory = new(3);

			for (int i = 0; i < inventory.Size; i++)
			{
				inventory.TrySwap(i, new ItemStack(items[i]), out _, forceSwap: true);
			}

			Assert.AreEqual(1, inventory.FreeSlots);
			Assert.AreEqual(2, inventory.OccupiedSlots);

			for (int i = 0; i < inventory.Size; i++)
			{
				ItemStack slot = inventory[i];
				int freeSlots = inventory.FreeSlots;
				int occupiedSlots = inventory.OccupiedSlots;

				if (slot.Item != null && slot.Count >= stackSize)
				{
					Assert.IsTrue(inventory.CanRemove(i, stackSize, out ItemStack resultCan, out ItemStack removedCan));
					Assert.IsTrue(inventory.TryRemove(i, stackSize, out ItemStack resultTry, out ItemStack removedTry));
					Assert.AreEqual(resultCan, resultTry);
					Assert.AreEqual(new ItemStack(slot.Item, stackSize), removedCan);
					Assert.AreEqual(new ItemStack(slot.Item, stackSize), removedTry);

					Assert.AreEqual(freeSlots + 1, inventory.FreeSlots);
					Assert.AreEqual(occupiedSlots - 1, inventory.OccupiedSlots);
				}
				else
				{
					Assert.IsFalse(inventory.CanRemove(i, stackSize, out ItemStack resultCan, out ItemStack removedCan));
					Assert.IsFalse(inventory.TryRemove(i, stackSize, out ItemStack resultTry, out ItemStack removedTry));
					Assert.AreEqual(resultCan, resultTry);
					Assert.AreEqual(default(ItemStack), removedCan);
					Assert.AreEqual(default(ItemStack), removedTry);

					Assert.AreEqual(freeSlots, inventory.FreeSlots);
					Assert.AreEqual(occupiedSlots, inventory.OccupiedSlots);
				}

				Assert.AreEqual(inventory.Count(s => s == default), inventory.FreeSlots);
				Assert.AreEqual(inventory.Count(s => s != default), inventory.OccupiedSlots);
			}
		}

		[Test]
		public void Remove_Slot_Full()
		{
			Inventory inventory = new(3);

			for (int i = 0; i < inventory.Size; i++)
			{
				inventory.TrySwap(i, new ItemStack(items[i]), out _, forceSwap: true);
			}

			for (int i = 0; i < inventory.Size; i++)
			{
				ItemStack slot = inventory[i];
				int freeSlots = inventory.FreeSlots;
				int occupiedSlots = inventory.OccupiedSlots;

				if (slot.Item != null)
				{
					Assert.IsTrue(inventory.CanRemove(i, out ItemStack resultCan, out ItemStack removedCan));
					Assert.IsTrue(inventory.TryRemove(i, out ItemStack resultTry, out ItemStack removedTry));
					Assert.AreEqual(resultCan, resultTry);
					Assert.AreEqual(slot, removedCan);
					Assert.AreEqual(slot, removedTry);

					Assert.AreEqual(freeSlots + 1, inventory.FreeSlots);
					Assert.AreEqual(occupiedSlots - 1, inventory.OccupiedSlots);
				}
				else
				{
					Assert.IsFalse(inventory.CanRemove(i, out ItemStack resultCan, out ItemStack removedCan));
					Assert.IsFalse(inventory.TryRemove(i, out ItemStack resultTry, out ItemStack removedTry));
					Assert.AreEqual(resultCan, resultTry);
					Assert.AreEqual(default(ItemStack), removedCan);
					Assert.AreEqual(default(ItemStack), removedTry);

					Assert.AreEqual(freeSlots, inventory.FreeSlots);
					Assert.AreEqual(occupiedSlots, inventory.OccupiedSlots);
				}

				Assert.AreEqual(inventory.Count(s => s == default), inventory.FreeSlots);
				Assert.AreEqual(inventory.Count(s => s != default), inventory.OccupiedSlots);
			}
		}

		[Test]
		public void Move_SameInventory()
		{
			Inventory inventory = new(new ItemStack[] { new(items[1]), new(items[1]), new(items[2]) });

			Assert.AreEqual(0, inventory.FreeSlots);
			Assert.AreEqual(3, inventory.OccupiedSlots);

			// Move
			Assert.IsTrue(inventory.TryMove(0, inventory, 1, allowSwap: false));
			// Check Items
			Assert.AreEqual(new ItemStack(items[1], 2), inventory[0]);
			Assert.AreEqual(default(ItemStack), inventory[1]);
			// Check Free/Occupied Slots
			Assert.AreEqual(1, inventory.FreeSlots);
			Assert.AreEqual(2, inventory.OccupiedSlots);

			// Move
			Assert.IsTrue(inventory.TryMove(1, inventory, 2, allowSwap: false));
			// Check Items
			Assert.AreEqual(new ItemStack(items[2]), inventory[1]);
			Assert.AreEqual(default(ItemStack), inventory[2]);
			// Check Free/Occupied Slots
			Assert.AreEqual(1, inventory.FreeSlots);
			Assert.AreEqual(2, inventory.OccupiedSlots);

			// Move
			Assert.IsFalse(inventory.TryMove(0, inventory, 1, allowSwap: false));
			Assert.IsTrue(inventory.TryMove(0, inventory, 1, allowSwap: true));
			// Check Items
			Assert.AreEqual(new ItemStack(items[2]), inventory[0]);
			Assert.AreEqual(new ItemStack(items[1], 2), inventory[1]);
			Assert.AreEqual(default(ItemStack), inventory[2]);
			// Check Free/Occupied Slots
			Assert.AreEqual(1, inventory.FreeSlots);
			Assert.AreEqual(2, inventory.OccupiedSlots);
		}

		[Test]
		public void Move_DifferentInventories()
		{
			Inventory inventory1 = new(new ItemStack[] { new(items[1]), new(items[1]), new(items[2]) });
			Inventory inventory2 = new(new ItemStack[] { new(items[1]), new(items[1], items[1].MaxStacks), new(items[2]) });

			Assert.AreEqual(0, inventory1.FreeSlots);
			Assert.AreEqual(3, inventory1.OccupiedSlots);

			Assert.AreEqual(0, inventory2.FreeSlots);
			Assert.AreEqual(3, inventory2.OccupiedSlots);

			// Move
			Assert.IsTrue(inventory1.TryMove(0, inventory2, 0, allowSwap: false));
			// Check Items
			Assert.AreEqual(new ItemStack(items[1], 2), inventory1[0]);
			Assert.AreEqual(default(ItemStack), inventory2[0]);
			// Check Free/Occupied Slots
			// Target inventory
			Assert.AreEqual(0, inventory1.FreeSlots);
			Assert.AreEqual(3, inventory1.OccupiedSlots);
			// Source Inventory
			Assert.AreEqual(1, inventory2.FreeSlots);
			Assert.AreEqual(2, inventory2.OccupiedSlots);

			// Move
			Assert.IsTrue(inventory1.TryMove(0, inventory2, 1, allowSwap: false));
			// Check Items
			Assert.AreEqual(new ItemStack(items[1], items[1].MaxStacks), inventory1[0]);
			Assert.AreEqual(new ItemStack(items[1], 2), inventory2[1]);
			// Check Free/Occupied Slots
			// Target inventory
			Assert.AreEqual(0, inventory1.FreeSlots);
			Assert.AreEqual(3, inventory1.OccupiedSlots);
			// Source Inventory
			Assert.AreEqual(1, inventory2.FreeSlots);
			Assert.AreEqual(2, inventory2.OccupiedSlots);

			// Move
			Assert.IsFalse(inventory1.TryMove(1, inventory2, 2, allowSwap: false));
			// Check Items
			Assert.AreEqual(new ItemStack(items[1]), inventory1[1]);
			Assert.AreEqual(new ItemStack(items[2]), inventory1[2]);
			// Check Free/Occupied Slots
			// Target inventory
			Assert.AreEqual(0, inventory1.FreeSlots);
			Assert.AreEqual(3, inventory1.OccupiedSlots);
			// Source Inventory
			Assert.AreEqual(1, inventory2.FreeSlots);
			Assert.AreEqual(2, inventory2.OccupiedSlots);

			// Move
			Assert.IsTrue(inventory1.TryMove(1, inventory2, 2, allowSwap: true));
			// Check Items
			Assert.AreEqual(new ItemStack(items[2]), inventory1[1]);
			Assert.AreEqual(new ItemStack(items[1]), inventory2[2]);
			// Check Free/Occupied Slots
			// Target inventory
			Assert.AreEqual(0, inventory1.FreeSlots);
			Assert.AreEqual(3, inventory1.OccupiedSlots);
			// Source Inventory
			Assert.AreEqual(1, inventory2.FreeSlots);
			Assert.AreEqual(2, inventory2.OccupiedSlots);
		}

		[Test]
		public void CopyFrom_Array([ValueSource(inventorySizesSrc)] int size)
		{
			// Arrange
			ItemStack[] itemStacks = new ItemStack[] { default, new(items[1]), new(items[2]) };
			Inventory inventory = new(size);

			// Act
			inventory.CopyFrom(itemStacks);

			// Assert
			for (int i = 0; i < inventory.Size && i < itemStacks.Length; i++)
			{
				Assert.AreEqual(itemStacks[i], inventory[i]);
			}

			Assert.AreEqual(Math.Max(size, itemStacks.Length) - 2, inventory.FreeSlots, 0);
			Assert.AreEqual(2, inventory.OccupiedSlots, 0);
		}

		[Test]
		public void CopyFrom_Span([ValueSource(inventorySizesSrc)] int size)
		{
			// Arrange
			ItemStack[] itemStacks = new ItemStack[] { default, new(items[1]), new(items[2]) };
			Inventory inventory = new(size);

			// Act
			inventory.CopyFrom(itemStacks.AsSpan());

			// Assert
			for (int i = 0; i < inventory.Size && i < itemStacks.Length; i++)
			{
				Assert.AreEqual(itemStacks[i], inventory[i]);
			}

			Assert.AreEqual(Math.Max(size, itemStacks.Length) - 2, inventory.FreeSlots, 0);
			Assert.AreEqual(2, inventory.OccupiedSlots, 0);
		}

		[Test]
		public void CopyFrom_List([ValueSource(inventorySizesSrc)] int size)
		{
			// Arrange
			ItemStack[] itemStacks = new ItemStack[] { default, new(items[1]), new(items[2]) };
			Inventory inventory = new(size);

			// Act
			inventory.CopyFrom((IReadOnlyList<ItemStack>)itemStacks);

			// Assert
			for (int i = 0; i < inventory.Size && i < itemStacks.Length; i++)
			{
				Assert.AreEqual(itemStacks[i], inventory[i]);
			}

			Assert.AreEqual(Math.Max(size, itemStacks.Length) - 2, inventory.FreeSlots, 0);
			Assert.AreEqual(2, inventory.OccupiedSlots, 0);
		}

		[Test]
		public void Sort([ValueSource(inventorySizesSrc)] int size)
		{
			// Arrange
			ItemStack[] itemStacks = new ItemStack[] { default, new(items[1]), new(items[2]) };
			Inventory inventory = new(size);
			inventory.CopyFrom(itemStacks);

			// Act
			inventory.Sort();

			// Assert
			Assert.IsTrue(IsSorted(inventory, Comparer<ItemStack>.Default));
		}

		[Test]
		public void Clear([ValueSource(inventorySizesSrc)] int size)
		{
			// Arrange
			ItemStack[] itemStacks = new ItemStack[] { default, new(items[1]), new(items[2]) };
			Inventory inventory = new(size);
			inventory.CopyFrom(itemStacks);

			// Act
			inventory.Clear();

			// Assert
			for (int i = 0; i < inventory.Size; i++)
			{
				Assert.AreEqual(default(ItemStack), inventory[i]);
			}

			Assert.AreEqual(Math.Max(size, itemStacks.Length), inventory.FreeSlots, 0);
			Assert.AreEqual(0, inventory.OccupiedSlots, 0);
		}

		private static bool IsSorted<T, TComparer>(IReadOnlyList<T> data, TComparer comparer) where TComparer : IComparer<T> => IsSorted(data, 0, data.Count, comparer);
		private static bool IsSorted<T, TComparer>(IReadOnlyList<T> data, int index, int length, TComparer comparer) where TComparer : IComparer<T>
		{
			for (int i = index + 1; i < index + length; i++)
			{
				if (comparer.Compare(data[i - 1], data[i]) > 0)
				{
					return false;
				}
			}
			return true;
		}
	}
}