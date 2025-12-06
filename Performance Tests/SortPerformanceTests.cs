using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kryz.RPG.Items.Interfaces;
using Kryz.RPG.Items.Structs;
using Kryz.Utils;
using UnityEngine;
using UnityEngine.Assertions;

public class SortPerformanceTests : MonoBehaviour
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

	[SerializeField] int numItems = 500;

	private int[] shuffledInts;
	private TestItem[] shuffledItems;
	private ItemStack[] shuffledItemStacks;

	private void Awake()
	{
		shuffledInts = new int[numItems];
		shuffledItems = new TestItem[numItems];
		shuffledItemStacks = new ItemStack[numItems];

		for (int i = 0; i < shuffledItemStacks.Length; i++)
		{
			shuffledInts[i] = i;
			shuffledItems[i] = new TestItem(i);
			shuffledItemStacks[i] = new ItemStack(new TestItem(i), 20);
		}

		Shuffle(shuffledInts);
		Shuffle(shuffledItems);
		Shuffle(shuffledItemStacks);

		TestSort();
	}

	private void Update()
	{
		int[] ints = new int[numItems];

		Array.Copy(shuffledInts, ints, numItems);
		ArraySortInt(ints);
		Array.Copy(shuffledInts, ints, numItems);
		TimSortLiteInt(ints);

		TestItem[] items = new TestItem[numItems];

		Array.Copy(shuffledItems, items, numItems);
		ArraySortTestItem(items);
		Array.Copy(shuffledItems, items, numItems);
		TimSortLiteTestItem(items);

		ItemStack[] itemStacks = new ItemStack[numItems];

		Array.Copy(shuffledItemStacks, itemStacks, numItems);
		ArraySortItemStack(itemStacks);
		Array.Copy(shuffledItemStacks, itemStacks, numItems);
		TimSortLiteItemStack(itemStacks);
	}

	private static void ArraySortInt(int[] data) => Array.Sort(data);
	private static void TimSortLiteInt(int[] data) => TimSortLite.Sort(data);

	private static void ArraySortTestItem(TestItem[] data) => Array.Sort(data);
	private static void TimSortLiteTestItem(TestItem[] data) => TimSortLite.Sort(data);

	private static void ArraySortItemStack(ItemStack[] data) => Array.Sort(data);
	private static void TimSortLiteItemStack(ItemStack[] data) => TimSortLite.Sort(data);

	private void TestSort()
	{
		int[] sorted = Enumerable.Range(0, 100).ToArray();
		int[] data = Enumerable.Range(0, 100).ToArray();
		Shuffle(data);

		int[] sortTest1 = new int[data.Length];
		int[] sortTest2 = new int[data.Length];
		int[] sortTest3 = new int[data.Length];
		int[] sortTest4 = new int[data.Length];
		int[] sortTest5 = new int[data.Length];
		int[] sortTest6 = new int[data.Length];

		Array.Copy(data, sortTest1, data.Length);
		Array.Copy(data, sortTest2, data.Length);
		Array.Copy(data, sortTest3, data.Length);
		Array.Copy(data, sortTest4, data.Length);
		Array.Copy(data, sortTest5, data.Length);
		Array.Copy(data, sortTest6, data.Length);

		InsertionSort.Sort(sortTest1);
		BinarySort.Sort(sortTest2);
		MergeSort.Sort(sortTest3);
		MergeBinarySort.Sort(sortTest4);
		TimSortLite.Sort(sortTest5);
		TimSortLite.Sort(sortTest6.AsSpan());

		Debug.Log("Data: " + ToString(data));
		Debug.Log("Ins: " + ToString(sortTest1));
		Debug.Log("Bin: " + ToString(sortTest2));
		Debug.Log("Mer: " + ToString(sortTest3));
		Debug.Log("MeB: " + ToString(sortTest4));
		Debug.Log("Tim: " + ToString(sortTest5));
		Debug.Log("Tim Span: " + ToString(sortTest6));

		Assert.IsTrue(sorted.ContentEquals(sortTest1));
		Assert.IsTrue(sorted.ContentEquals(sortTest2));
		Assert.IsTrue(sorted.ContentEquals(sortTest3));
		Assert.IsTrue(sorted.ContentEquals(sortTest4));
		Assert.IsTrue(sorted.ContentEquals(sortTest5));
		Assert.IsTrue(sorted.ContentEquals(sortTest6));
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