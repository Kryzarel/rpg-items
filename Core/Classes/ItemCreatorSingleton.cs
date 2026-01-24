using System;
using System.Collections.Generic;

namespace Kryz.RPG.Items
{
	public abstract class ItemCreatorSingleton<TDefinition, TItem, TSaveData> : IItemCreator<TDefinition>, IItemSaver<TItem, TSaveData> where TDefinition : IItemDefinition where TItem : IItem where TSaveData : IItemSaveData
	{
		private readonly Dictionary<TDefinition, IItem> items = new();

		public bool CanCreate(IItemDefinition definition)
		{
			return definition is TDefinition;
		}

		public bool CanCreate(IItemDefinition definition, IItemSaveData saveData)
		{
			return definition is TDefinition;
		}

		public bool CanSave(IItem item)
		{
			return item is TItem;
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
			if (definition is not TDefinition)
			{
				throw new InvalidOperationException($"{GetType().Name} does not support {definition.GetType().Name}");
			}

			return Create(definition);
		}

		public IItemSaveData ToSaveData(IItem item)
		{
			if (item is not TItem itemCast)
			{
				throw new InvalidOperationException($"{GetType().Name} does not support saving {item.GetType().Name}");
			}

			return ToSaveData(itemCast);
		}

		public abstract IItem Create(TDefinition definition);

		public abstract TSaveData ToSaveData(TItem item);
	}
}