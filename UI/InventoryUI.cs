using System.Collections.Generic;
using Kryz.RPG.Items.Interfaces;
using UnityEngine;

namespace Kryz.RPG.Items.UI
{
	public class InventoryUI : MonoBehaviour
	{
		[SerializeField] ItemSlotUI itemSlotPrefab;
		[SerializeField] List<ItemSlotUI> itemSlots;

		private IInventory? inventory;

		public void SetInventory(IInventory inventory)
		{
			if (this.inventory != inventory)
			{
				this.inventory = inventory;
			}
		}
	}
}