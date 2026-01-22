using System;
using System.Collections.Generic;

namespace Kryz.RPG.Items
{
	public abstract class ItemCreatorSingleton<TDefinition> : IItemCreator<TDefinition> where TDefinition : IItemDefinition
	{
		private readonly Dictionary<TDefinition, IItem> items = new();

		public bool CanCreate(IItemDefinition definition)
		{
			return definition is TDefinition;
		}

		public bool CanCreate(IItemDefinition definition, IItemSaveData saveData)
		{
			return definition is TDefinition && saveData is ItemSaveDataSingleton;
		}

		public IItem Create(IItemDefinition definition)
		{
			if (definition is not TDefinition itemDefinition)
			{
				throw new InvalidOperationException($"{GetType().Name} does not support {definition.GetType().Name}");
			}

			if (!items.TryGetValue(itemDefinition, out IItem item))
			{
				item = items[itemDefinition] = Create(itemDefinition);
			}
			return item;
		}

		public IItem Create(IItemDefinition definition, IItemSaveData saveData)
		{
			if (definition is not TDefinition || saveData is not ItemSaveDataSingleton)
			{
				throw new InvalidOperationException($"{GetType().Name} does not support {definition.GetType().Name}");
			}

			return Create(definition);
		}

		public abstract IItem Create(TDefinition definition);
	}
}