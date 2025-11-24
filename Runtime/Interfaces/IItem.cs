using UnityEngine;

namespace Kryz.RPG.Items.Interfaces
{
	public interface IItem
	{
		public Sprite Icon { get; }
		public string Name { get; }
		public string Description { get; }
		public int MaxStacks { get; }
	}
}