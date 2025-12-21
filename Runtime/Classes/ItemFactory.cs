using System;
using System.Collections.Generic;
using Kryz.RPG.Items.Interfaces;

namespace Kryz.RPG.Items.Classes
{
	public class ItemFactory : IItemFactory
	{
		private readonly Dictionary<Type, IItemCreator> itemCreators = new();

		public IItem GetOrCreate<TDefinition>(TDefinition definition) where TDefinition : IItemDefinition
		{
			IItemCreator<TDefinition> creator = (IItemCreator<TDefinition>)itemCreators[typeof(TDefinition)];
			return creator.GetOrCreate(definition);
		}

		public void Register<TDefinition>(IItemCreator<TDefinition> creator) where TDefinition : IItemDefinition
		{
			itemCreators[typeof(TDefinition)] = creator;
		}
	}
}