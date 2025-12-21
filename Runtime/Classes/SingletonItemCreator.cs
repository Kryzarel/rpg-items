using System.Collections.Generic;

namespace Kryz.RPG.Items
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