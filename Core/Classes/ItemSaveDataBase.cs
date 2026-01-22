using System;
using UnityEngine;

namespace Kryz.RPG.Items
{
	[Serializable]
	public abstract class ItemSaveDataBase : IItemSaveData
	{
		[field: SerializeField]
		public string DefinitionId { get; private set; }

		public ItemSaveDataBase(string definitionId)
		{
			DefinitionId = definitionId;
		}
	}
}