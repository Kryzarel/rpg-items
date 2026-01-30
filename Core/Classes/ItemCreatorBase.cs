using System;

namespace Kryz.RPG.Items
{
	public abstract class ItemCreatorBase<TDefinition, TItem, TSaveData> : IItemCreator<TDefinition, TSaveData>, IItemSaver<TItem, TSaveData> where TDefinition : IItemDefinition where TItem : IItem where TSaveData : IItemSaveData
	{
		public bool CanCreate(IItemDefinition definition)
		{
			return definition is TDefinition;
		}

		public bool CanCreate(IItemDefinition definition, IItemSaveData saveData)
		{
			return definition is TDefinition && saveData is TSaveData;
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

			return Create(itemDefinition);
		}

		public IItem Create(IItemDefinition definition, IItemSaveData saveData)
		{
			if (definition is not TDefinition itemDefinition || saveData is not TSaveData itemSaveData)
			{
				throw new InvalidOperationException($"{GetType().Name} does not support pair ({definition.GetType().Name}, {saveData.GetType().Name})");
			}

			return Create(itemDefinition, itemSaveData);
		}

		public IItemSaveData ToSaveData(IItem item)
		{
			if (item is not TItem itemCast)
			{
				throw new InvalidOperationException($"{GetType().Name} does not support saving {item.GetType().Name}");
			}

			return ToSaveData(itemCast);
		}

		IItem IItemCreator<TDefinition, TSaveData>.Create(TDefinition definition, TSaveData saveData)
		{
			return Create(definition, saveData);
		}

		IItem IItemCreator<TDefinition>.Create(TDefinition definition)
		{
			return Create(definition);
		}

		public abstract TItem Create(TDefinition definition);

		public abstract TItem Create(TDefinition definition, TSaveData saveData);

		public abstract TSaveData ToSaveData(TItem item);
	}
}