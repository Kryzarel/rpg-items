using System;

namespace Kryz.RPG.Items
{
	[Serializable]
	public sealed class ItemSaveDataSingleton : ItemSaveDataBase
	{
		public ItemSaveDataSingleton(string definitionId) : base(definitionId) { }
	}
}