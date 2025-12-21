namespace Kryz.RPG.Items
{
	public interface IItemFactory
	{
		IItem GetOrCreate<TDefinition>(TDefinition definition) where TDefinition : IItemDefinition;
	}
}