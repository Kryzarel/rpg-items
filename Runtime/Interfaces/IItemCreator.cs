namespace Kryz.RPG.Items.Interfaces
{
	public interface IItemCreator<in TDefinition> where TDefinition : IItemDefinition
	{
		IItem GetOrCreate(TDefinition definition);
	}
}