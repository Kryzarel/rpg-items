using System;
using UnityEngine;

namespace Kryz.RPG.Items
{
	public interface IItem : IComparable<IItem>
	{
		string Name { get; }
		Sprite Icon { get; }
		int MaxStacks { get; }
		IItemDefinition Definition { get; }
	}
}