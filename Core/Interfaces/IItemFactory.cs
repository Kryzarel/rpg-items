namespace Kryz.RPG.Items
{
	public interface IItemFactory
	{
		IItem Create(IItemDefinition definition);
		IItem CreateFromSave(IItemSaveData saveData);
		IItemSaveData ToSaveData(IItem item);
	}
}