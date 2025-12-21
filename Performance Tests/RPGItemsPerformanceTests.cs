using System.Collections.Generic;
using Kryz.RPG.Items.Classes;
using Kryz.RPG.Items.Interfaces;
using Kryz.RPG.Items.Structs;
using UnityEngine;

public class RPGItemsPerformanceTests : MonoBehaviour
{
	private class TestItem : IItem
	{
		private readonly int index;
		private readonly string name;

		public IItemDefinition Definition => null!;
		public Sprite Icon => null!;
		public string Name => name;
		public string Description => "An item just for testing";
		public int MaxStacks => 20;

		public int CompareTo(IItem other) => other is TestItem test ? index.CompareTo(test.index) : 0;
		public override string ToString() => Name;
		public TestItem(int index)
		{
			this.index = index;
			name = $"Test Item {index}";
		}
	}

	[SerializeField] int inventorySize = 100;
	[SerializeField] int numItems = 100;

	private ItemStack[] itemsToAdd;
	private Inventory inventory;

	private void Awake()
	{
		itemsToAdd = new ItemStack[numItems];
		for (int i = 0; i < itemsToAdd.Length; i++)
		{
			itemsToAdd[i] = new ItemStack(new TestItem(i), 20);
		}

		Shuffle(itemsToAdd);

		inventory = new(inventorySize);
	}

	private void Update()
	{
		// inventory.CopyFrom(itemsToAdd);
		// inventory.Sort();

		Enumerate();
		EnumerateInterface();
	}

	private void Enumerate()
	{
		foreach (ItemStack item in inventory)
		{
		}
	}

	private void EnumerateInterface()
	{
		foreach (ItemStack item in (IEnumerable<ItemStack>)inventory)
		{
		}
	}

	public static void Shuffle<T>(T[] array)
	{
		int n = array.Length;
		while (n > 1)
		{
			int k = Random.Range(0, n--);
			(array[k], array[n]) = (array[n], array[k]);
		}
	}
}