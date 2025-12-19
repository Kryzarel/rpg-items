namespace Kryz.RPG.Items.Interfaces
{
	public interface IItemDefinition
	{
		IItem CreateWith(IItemFactory factory);
	}
}