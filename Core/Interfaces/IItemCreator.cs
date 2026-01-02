namespace Kryz.RPG.Items
{
	public interface IItemCreator { }

	public interface IItemCreator<in TDefinition> : IItemCreator where TDefinition : IItemDefinition
	{
		IItem GetOrCreate(TDefinition definition);
	}
}