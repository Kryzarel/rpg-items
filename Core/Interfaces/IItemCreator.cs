namespace Kryz.RPG.Items
{
	public interface IItemCreator
	{
		bool CanCreate(IItemDefinition definition);
		bool CanCreate(IItemDefinition definition, IItemSaveData saveData);

		IItem Create(IItemDefinition definition);
		IItem Create(IItemDefinition definition, IItemSaveData saveData);
	}

	public interface IItemCreator<in TDefinition> : IItemCreator where TDefinition : IItemDefinition
	{
		IItem Create(TDefinition definition);
	}

	public interface IItemCreator<in TDefinition, in TSaveData> : IItemCreator<TDefinition> where TDefinition : IItemDefinition where TSaveData : IItemSaveData
	{
		IItem Create(TDefinition definition, TSaveData saveData);
	}
}