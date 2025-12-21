using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kryz.RPG.Items.Interfaces;
using Kryz.RPG.Items.Structs;
using Kryz.Utils;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Assertions;

public class SortPerformanceTests : MonoBehaviour
{
	private class TestItem : IItem/* , IComparable<TestItem> */
	{
		private readonly int index;
		private readonly string name;

		public IItemDefinition Definition => null!;
		public Sprite Icon => null!;
		public string Name => name;
		public string Description => "An item just for testing";
		public int MaxStacks => 20;

		public int CompareTo(IItem other) => name.CompareTo(other.Name);
		// public int CompareTo(TestItem other) => index.CompareTo(other.index);
		public override string ToString() => Name;

		public TestItem(int index)
		{
			this.index = index;
			name = $"Test Item {index}";
		}
	}

	[SerializeField] int numItems = 500;
	[SerializeField] int numFramesToRun = 500;

	private int[] originalInts;
	private TestItem[] originalItems;
	private ItemStack[] originalItemStacks;

	private int[] ints;
	private TestItem[] items;
	private ItemStack[] itemStacks;

	private int startingFrame;

	private void Awake()
	{
		originalInts = new int[numItems];
		originalItems = new TestItem[numItems];
		originalItemStacks = new ItemStack[numItems];

		ints = new int[numItems];
		items = new TestItem[numItems];
		itemStacks = new ItemStack[numItems];

		for (int i = 0; i < originalItemStacks.Length; i++)
		{
			originalInts[i] = i;
			originalItems[i] = new TestItem(i);
			originalItemStacks[i] = new ItemStack(new TestItem(i), 20);
		}

		Shuffle(originalInts);
		Shuffle(originalItems);
		Shuffle(originalItemStacks);

		// TestSort();

		// Application.targetFrameRate = 60;
		// QualitySettings.vSyncCount = 0;
	}

	private void Start()
	{
		startingFrame = Time.frameCount;
	}

	private void Update()
	{
		if (numFramesToRun > 0 && Time.frameCount - startingFrame >= numFramesToRun)
		{
			Application.Quit();
			return;
		}

		TestSorts<int>.Run(originalInts, ints);
		TestSorts<TestItem>.Run(originalItems, items);
		// TestSorts<ItemStack>.Run(originalItemStacks, itemStacks);
	}

	private static class TestSorts<T>
	{
		private static readonly Action<T[]>[] sorts = { Array.Sort, /* InsertionSort.Sort, */ BinarySort.Sort, MergeSort.Sort, MergeBinarySort.Sort, TimSortLite.Sort };
		private static readonly ProfilerMarker[] markers = new ProfilerMarker[sorts.Length];

		static TestSorts()
		{
			for (int i = 0; i < sorts.Length; i++)
			{
				markers[i] = new($"{sorts[i].Method.DeclaringType.Name}<{typeof(T).Name}>");
			}
		}

		public static void Run(T[] original, T[] data)
		{
			for (int i = 0; i < sorts.Length; i++)
			{
				Array.Copy(original, data, original.Length);
				markers[i].Begin();
				sorts[i](data);
				markers[i].End();
			}
		}
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
		int[] sortTest5 = new int[data.Length];

		Array.Copy(data, sortTest1, data.Length);
		Array.Copy(data, sortTest2, data.Length);
		Array.Copy(data, sortTest3, data.Length);
		Array.Copy(data, sortTest4, data.Length);
		Array.Copy(data, sortTest5, data.Length);

		InsertionSort.Sort(sortTest1);
		BinarySort.Sort(sortTest2);
		MergeSort.Sort(sortTest3);
		MergeBinarySort.Sort(sortTest4);
		TimSortLite.Sort(sortTest5);

		Debug.Log("Data: " + ToString(data));
		Debug.Log("Ins: " + ToString(sortTest1));
		Debug.Log("Bin: " + ToString(sortTest2));
		Debug.Log("Mer: " + ToString(sortTest3));
		Debug.Log("MeB: " + ToString(sortTest4));
		Debug.Log("Tim: " + ToString(sortTest5));

		Assert.IsTrue(sorted.ContentEquals(sortTest1));
		Assert.IsTrue(sorted.ContentEquals(sortTest2));
		Assert.IsTrue(sorted.ContentEquals(sortTest3));
		Assert.IsTrue(sorted.ContentEquals(sortTest4));
		Assert.IsTrue(sorted.ContentEquals(sortTest5));
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