using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Kryz.RPG.Items
{
	public class ItemFactory : IItemFactory
	{
		private readonly Dictionary<Type, IItemCreator> itemCreators = new();
		private readonly IItemDefinitionDatabase definitionDatabase;

		public ItemFactory(IItemDefinitionDatabase database)
		{
			definitionDatabase = database;
		}

		public IItem Create(IItemDefinition definition)
		{
			if (!TryGetCreator(definition, out IItemCreator? creator))
			{
				throw new InvalidOperationException($"No {nameof(IItemCreator)} registered for {definition.GetType().Name}");
			}

			if (!creator.CanCreate(definition))
			{
				throw new InvalidOperationException($"{creator.GetType()} does not support {definition.GetType().Name}");
			}

			return creator.Create(definition);
		}

		public IItem CreateFromSave(IItemSaveData saveData)
		{
			if (!definitionDatabase.TryGet(saveData.DefinitionId, out IItemDefinition? definition))
			{
				throw new InvalidOperationException($"Invalid {saveData.GetType().Name}: Definition \"{saveData.DefinitionId}\" not registered in database");
			}

			if (!TryGetCreator(definition, out IItemCreator? creator))
			{
				throw new InvalidOperationException($"No {nameof(IItemCreator)} registered for {definition.GetType().Name}");
			}

			if (!creator.CanCreate(definition, saveData))
			{
				throw new InvalidOperationException($"Invalid {saveData.GetType().Name}: {creator.GetType()} does not support pair ({definition.GetType().Name}, {saveData.GetType().Name})");
			}

			return creator.Create(definition, saveData);
		}

		public void Register<TDefinition>(IItemCreator<TDefinition> creator) where TDefinition : IItemDefinition
		{
			itemCreators[typeof(TDefinition)] = creator;
		}

		private bool TryGetCreator(IItemDefinition definition, [MaybeNullWhen(returnValue: false)] out IItemCreator creator)
		{
			for (Type type = definition.GetType(); type != null; type = type.BaseType)
			{
				if (itemCreators.TryGetValue(type, out creator))
				{
					return true;
				}
			}

			creator = null;
			return false;
		}
	}
}