using System;

namespace Kryz.RPG.Items
{
	public abstract class ItemCreator<TDefinition, TSaveData> : IItemCreator<TDefinition, TSaveData> where TDefinition : IItemDefinition where TSaveData : IItemSaveData
	{
		public bool CanCreate(IItemDefinition definition)
		{
			return definition is TDefinition;
		}

		public bool CanCreate(IItemDefinition definition, IItemSaveData saveData)
		{
			return definition is TDefinition && saveData is TSaveData;
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

		public abstract IItem Create(TDefinition definition);

		public abstract IItem Create(TDefinition definition, TSaveData saveData);
	}
}