using UnityEngine;

namespace Kryz.RPG.Items.Tests
{
	public class TestItem : IItem
	{
		public IItemDefinition Definition => null!;
		public Sprite Icon => null!;
		public string Name { get; }
		public int MaxStacks { get; }

		public TestItem(string name, int maxStacks = 99)
		{
			Name = name;
			MaxStacks = maxStacks;
		}

		public int CompareTo(IItem other) => string.Compare(Name, other.Name);

		public override string ToString() => Name;
	}
}