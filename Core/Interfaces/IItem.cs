using System;
using UnityEngine;

namespace Kryz.RPG.Items
{
	public interface IItem : IComparable<IItem>
	{
		IItemDefinition Definition { get; }

		Sprite Icon { get; }
		string Name { get; }
		int MaxStacks { get; }

		IItemSaveData ToSaveData();
	}
}