namespace Kryz.RPG.Items
{
	public interface IItemDefinition
	{
		string Id { get; }
		string Name { get; }
		IItem CreateWith(IItemFactory factory);
	}
}