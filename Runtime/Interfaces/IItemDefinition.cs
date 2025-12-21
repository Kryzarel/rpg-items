namespace Kryz.RPG.Items
{
	public interface IItemDefinition
	{
		IItem CreateWith(IItemFactory factory);
	}
}