using System;
using Kryz.UnityUtils;
using UnityEngine;

namespace Kryz.RPG.Items
{
	[Serializable]
	public struct ItemStackSaveData
	{
		[SerializeReference, SerializeReferencePicker]
		public IItemSaveData? Item;

		[SerializeField]
		public int Count;

		public ItemStackSaveData(IItemSaveData? item, int count)
		{
			Item = item;
			Count = count;
		}
	}
}