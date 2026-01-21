using System;
using UnityEngine;

namespace Kryz.RPG.Items
{
	[Serializable]
	public abstract class BaseItemSaveData : IItemSaveData
	{
		[field: SerializeField]
		public string DefinitionId { get; private set; }

		public BaseItemSaveData(string definitionId)
		{
			DefinitionId = definitionId;
		}
	}
}