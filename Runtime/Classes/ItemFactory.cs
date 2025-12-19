using Kryz.DI;
using Kryz.RPG.Items.Interfaces;

namespace Kryz.RPG.Items
{
	public class ItemFactory : IItemFactory
	{
		private readonly IContainer container;

		public ItemFactory(IContainer container)
		{
			this.container = container;
		}

		public IItem GetOrCreate<TDefinition>(TDefinition definition) where TDefinition : IItemDefinition
		{
			return container.ResolveObject<IItemCreator<TDefinition>>().GetOrCreate(definition);
		}
	}
}