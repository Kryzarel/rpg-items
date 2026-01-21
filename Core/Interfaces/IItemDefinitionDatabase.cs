namespace Kryz.RPG.Items
{
	public interface IItemDefinitionDatabase
	{
		IItemDefinition Get(string id);

		bool TryGet(string id, out IItemDefinition definition);
	}
}