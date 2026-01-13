using System;

namespace Kryz.RPG.Items
{
	public interface IItem : IComparable<IItem>
	{
		string Name { get; }
		IItemDefinition Definition { get; }
	}
}