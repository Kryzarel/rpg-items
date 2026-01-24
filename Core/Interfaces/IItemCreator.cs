namespace Kryz.RPG.Items
{
	public interface IItemCreator
	{
		bool CanCreate(IItemDefinition definition);
		bool CanCreate(IItemDefinition definition, IItemSaveData saveData);
		bool CanSave(IItem item);

		IItem Create(IItemDefinition definition);
		IItem Create(IItemDefinition definition, IItemSaveData saveData);
		IItemSaveData ToSaveData(IItem item);
	}

	public interface IItemCreator<in TDefinition> : IItemCreator where TDefinition : IItemDefinition
	{
		IItem Create(TDefinition definition);
	}

	public interface IItemCreator<in TDefinition, in TSaveData> : IItemCreator<TDefinition> where TDefinition : IItemDefinition where TSaveData : IItemSaveData
	{
		IItem Create(TDefinition definition, TSaveData saveData);
	}

	public interface IItemSaver<in TItem, out TSaveData> where TItem : IItem where TSaveData : IItemSaveData
	{
		TSaveData ToSaveData(TItem item);
	}
}