using System;
using Kryz.RPG.Items.Interfaces;
using Kryz.RPG.Items.Structs;
using NUnit.Framework;
using UnityEngine;

namespace Kryz.RPG.Items.Tests
{
	public class ItemStackTests
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

		private const string itemsSouce = nameof(items);
		private const string stackSizesSouce = nameof(stackSizes);

		private static readonly IItem[] items = { null!, new TestItem(), new TestItem() };
		private static readonly int[] stackSizes = { -1, 0, 1, maxStacks / 2, maxStacks, maxStacks * 2 };

		[Test]
		public void Create([ValueSource(itemsSouce)] IItem item, [ValueSource(stackSizesSouce)] int stackSize)
		{
			ItemStack stack = new(item, stackSize);

			if (item == null || stackSize <= 0)
			{
				Assert.AreEqual(default(ItemStack), stack);
			}
			else
			{
				Assert.AreEqual(stack.Item, item);
				Assert.AreEqual(stack.Count, stackSize, 0);
			}
		}

		[Test]
		public void Add([ValueSource(itemsSouce)] IItem item1, [ValueSource(itemsSouce)] IItem item2, [ValueSource(stackSizesSouce)] int stackSize1, [ValueSource(stackSizesSouce)] int stackSize2)
		{
			ItemStack stack1 = new(item1, stackSize1);
			ItemStack stack2 = new(item2, stackSize2);

			int maxCount = item2?.MaxStacks ?? 0;

			bool success = stack1.TryAdd(stack2, out ItemStack result, out ItemStack remainder);

			if (stack2 != default && (stack1.Item == stack2.Item && stack1.Count < maxCount || stack1 == default))
			{
				int resultCount = Math.Min(stack1.Count + stack2.Count, maxCount);
				int remainderCount = Math.Max(stack1.Count + stack2.Count - maxCount, 0);

				Assert.AreEqual(result.Count, resultCount, 0);
				Assert.AreEqual(remainder.Count, remainderCount, 0);
			}
			else
			{
				Assert.IsFalse(success);
				Assert.IsTrue(result == stack1);
				Assert.IsTrue(stack2 == remainder);
			}

			ItemStack result2 = stack1.Add(stack2, out ItemStack remainder2);
			Assert.IsTrue(result2 == result);
			Assert.IsTrue(remainder2 == remainder);
		}

		[Test]
		public void Remove([ValueSource(itemsSouce)] IItem item, [ValueSource(stackSizesSouce)] int stackSize, [ValueSource(stackSizesSouce)] int amountToRemove)
		{
			ItemStack stack = new(item, stackSize);

			bool success = stack.TryRemove(amountToRemove, out ItemStack result, out ItemStack removedStack);

			if (amountToRemove > 0 && amountToRemove <= stack.Count && stack != default)
			{
				int resultCount = Math.Max(stack.Count - amountToRemove, 0);
				int removedCount = Math.Min(stack.Count, amountToRemove);

				Assert.AreEqual(result.Count, resultCount, 0);
				Assert.AreEqual(removedStack.Count, removedCount, 0);
			}
			else
			{
				Assert.IsFalse(success);
				Assert.IsTrue(result == stack);
				Assert.IsTrue(removedStack == default);
			}

			ItemStack result2 = stack.Remove(amountToRemove, out ItemStack removedStack2);
			Assert.IsTrue(result2 == result);
			Assert.IsTrue(removedStack2 == removedStack);
		}
	}
}