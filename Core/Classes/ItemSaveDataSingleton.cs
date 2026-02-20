using System;

namespace Kryz.RPG.Items
{
	[Serializable]
	public sealed class ItemSaveDataSingleton : ItemSaveData
	{
		public ItemSaveDataSingleton(string definitionId) : base(definitionId) { }
	}
}