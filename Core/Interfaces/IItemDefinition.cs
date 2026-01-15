namespace Kryz.RPG.Items
{
	public interface IItemDefinition
	{
		string Id { get; }
		IItem CreateWith(IItemFactory factory);
	}
}