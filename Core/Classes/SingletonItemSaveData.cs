using System;

namespace Kryz.RPG.Items
{
	[Serializable]
	public sealed class SingletonItemSaveData : BaseItemSaveData
	{
		public SingletonItemSaveData(string definitionId) : base(definitionId) { }
	}
}