using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Kryz.RPG.Items
{
	public class ItemDefinitionDatabaseResources : IItemDefinitionDatabase
	{
		public IItemDefinition Get(string id)
		{
			UnityEngine.Object obj = Resources.Load(id);

			if (obj == null)
			{
				throw new InvalidOperationException($"No {nameof(IItemDefinition)} for ID \"{id}\" was found in {nameof(Resources)}");
			}

			if (obj is not IItemDefinition definition)
			{
				throw new InvalidCastException($"Loaded asset {obj.name} cannot be cast to {nameof(IItemDefinition)}");
			}

			return definition;
		}

		public bool TryGet(string id, [MaybeNullWhen(returnValue: false)] out IItemDefinition definition)
		{
			UnityEngine.Object obj = Resources.Load(id);

			if (obj == null)
			{
				definition = null;
				return false;
			}

			if (obj is not IItemDefinition def)
			{
				definition = null;
				return false;
			}

			definition = def;
			return true;
		}
	}
}