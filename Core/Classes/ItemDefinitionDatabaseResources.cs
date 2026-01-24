using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Kryz.RPG.Items
{
	public class ItemDefinitionDatabaseResources : IItemDefinitionDatabase
	{
		public IItemDefinition Get(string id)
		{
			UnityEngine.Object obj = Resources.Load(id, typeof(IItemDefinition));

			if (obj == null)
			{
				throw new InvalidOperationException($"No {nameof(IItemDefinition)} for ID \"{id}\" was found in {nameof(Resources)}");
			}

			return (IItemDefinition)obj;
		}

		public bool TryGet(string id, [MaybeNullWhen(returnValue: false)] out IItemDefinition definition)
		{
			definition = Resources.Load(id, typeof(IItemDefinition)) as IItemDefinition;
			return definition != null;
		}
	}
}