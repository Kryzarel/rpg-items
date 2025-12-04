using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kryz.RPG.Items.Classes;
using Kryz.RPG.Items.Interfaces;
using Kryz.RPG.Items.Structs;
using Kryz.Utils;
using UnityEngine;
using UnityEngine.Assertions;

public class RPGItemsPerformanceTests : MonoBehaviour
{
	private class TestItem : IItem
	{
		private readonly int index;
		private readonly string name;

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

		TestSort();
		TestSortItems();
	}

	private void Update()
	{
		inventory.CopyFrom(itemsToAdd);
		inventory.Sort();

		// inventory.CopyFrom(itemsToAdd);
		// inventory.SortDefaultArray();
	}

	private void TestSort()
	{
		int[] sorted = Enumerable.Range(0, 100).ToArray();
		int[] data = Enumerable.Range(0, 100).ToArray();
		Shuffle(data);

		int[] sortTest1 = new int[data.Length];
		int[] sortTest2 = new int[data.Length];
		int[] sortTest3 = new int[data.Length];
		int[] sortTest4 = new int[data.Length];

		Array.Copy(data, sortTest1, data.Length);
		Array.Copy(data, sortTest2, data.Length);
		Array.Copy(data, sortTest3, data.Length);
		Array.Copy(data, sortTest4, data.Length);

		InsertionSort.Sort(sortTest1);
		BinarySort.Sort(sortTest2);
		MergeSort.Sort(sortTest3);
		MergeBinarySort.Sort(sortTest4);

		Debug.Log("Data: " + ToString(data));
		Debug.Log("Ins: " + ToString(sortTest1));
		Debug.Log("Bin: " + ToString(sortTest2));
		Debug.Log("Mer: " + ToString(sortTest3));
		Debug.Log("Com: " + ToString(sortTest4));

		Assert.IsTrue(sorted.ContentEquals(sortTest1));
		Assert.IsTrue(sorted.ContentEquals(sortTest2));
		Assert.IsTrue(sorted.ContentEquals(sortTest3));
		Assert.IsTrue(sorted.ContentEquals(sortTest4));
	}

	private void TestSortItems()
	{
		ItemStack[] sorted = new ItemStack[1000];
		for (int i = 0; i < sorted.Length; i++)
		{
			sorted[i] = new ItemStack(new TestItem(i));
		}

		ItemStack[] data = new ItemStack[sorted.Length];
		Array.Copy(sorted, data, data.Length);
		Shuffle(data);

		ItemStack[] sortTest1 = new ItemStack[data.Length];
		ItemStack[] sortTest2 = new ItemStack[data.Length];
		ItemStack[] sortTest3 = new ItemStack[data.Length];
		ItemStack[] sortTest4 = new ItemStack[data.Length];

		Array.Copy(data, sortTest1, data.Length);
		Array.Copy(data, sortTest2, data.Length);
		Array.Copy(data, sortTest3, data.Length);
		Array.Copy(data, sortTest4, data.Length);

		InsertionSort.Sort(sortTest1);
		BinarySort.Sort(sortTest2);
		MergeSort.Sort(sortTest3);
		MergeBinarySort.Sort(sortTest4);

		Debug.Log("Data: " + ToString(data));
		Debug.Log("Ins: " + ToString(sortTest1));
		Debug.Log("Bin: " + ToString(sortTest2));
		Debug.Log("Mer: " + ToString(sortTest3));
		Debug.Log("Com: " + ToString(sortTest4));

		Assert.IsTrue(sorted.ContentEquals(sortTest1));
		Assert.IsTrue(sorted.ContentEquals(sortTest2));
		Assert.IsTrue(sorted.ContentEquals(sortTest3));
		Assert.IsTrue(sorted.ContentEquals(sortTest4));
	}

	private static readonly StringBuilder sb = new();
	private static string ToString<T>(IReadOnlyList<T> list) where T : notnull
	{
		sb.Append('{').Append(' ');
		for (int i = 0; i < list.Count; i++)
		{
			sb.Append(list[i].ToString()).Append(',').Append(' ');
		}
		sb.Length -= 2;
		sb.Append(' ').Append('}');

		string s = sb.ToString();
		sb.Clear();
		return s;
	}

	public static void Shuffle<T>(T[] array)
	{
		int n = array.Length;
		while (n > 1)
		{
			int k = UnityEngine.Random.Range(0, n--);
			(array[k], array[n]) = (array[n], array[k]);
		}
	}
}