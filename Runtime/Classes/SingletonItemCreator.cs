using System.Collections.Generic;
using Kryz.RPG.Items.Interfaces;

namespace Kryz.RPG.Items.Classes
{
	public abstract class SingletonItemCreator<TDefinition> : IItemCreator<TDefinition> where TDefinition : IItemDefinition
	{
		private readonly Dictionary<TDefinition, IItem> items = new();

		public IItem GetOrCreate(TDefinition definition)
		{
			if (!items.TryGetValue(definition, out IItem item))
			{
				item = items[definition] = Create(definition);
			}
			return item;
		}

		protected abstract IItem Create(TDefinition definition);
	}
}