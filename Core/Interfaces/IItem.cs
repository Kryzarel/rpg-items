using System;
using UnityEngine;

namespace Kryz.RPG.Items
{
	public interface IItem : IComparable<IItem>
	{
		Sprite Icon { get; }
		string Name { get; }
		int MaxStacks { get; }
		IItemDefinition Definition { get; }
	}
}