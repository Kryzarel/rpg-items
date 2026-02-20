using System;
using UnityEngine;

namespace Kryz.RPG.Items
{
	[Serializable]
	public abstract class ItemSaveData : IItemSaveData
	{
		[field: SerializeField]
		public string DefinitionId { get; private set; }

		public ItemSaveData(string definitionId)
		{
			DefinitionId = definitionId;
		}
	}
}