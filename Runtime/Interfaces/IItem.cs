using System;
using UnityEngine;

namespace Kryz.RPG.Items.Interfaces
{
	public interface IItem : IComparable<IItem>
	{
		IItemDefinition Definition { get; }
		Sprite Icon { get; }
		string Name { get; }
		string Description { get; }
		int MaxStacks { get; }
	}
}