namespace Kryz.RPG.Items.Interfaces
{
	public interface IItemCreator { }

	public interface IItemCreator<in TDefinition> : IItemCreator where TDefinition : IItemDefinition
	{
		IItem GetOrCreate(TDefinition definition);
	}
}