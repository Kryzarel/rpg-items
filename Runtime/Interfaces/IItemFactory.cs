namespace Kryz.RPG.Items.Interfaces
{
	public interface IItemFactory
	{
		IItem GetOrCreate<TDefinition>(TDefinition definition) where TDefinition : IItemDefinition;
	}
}