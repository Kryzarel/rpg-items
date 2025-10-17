using System;
using Kryz.RPG.Items.Classes;
using Kryz.RPG.Items.Structs;
using NUnit.Framework;

namespace Kryz.RPG.Items.Tests
{
	public class InventoryTests
	{
		[Test]
		public void Iventory_Create()
		{
			Inventory inventory = new(0);

			Assert.AreEqual(inventory.NumSlots, 0, 0);
			Assert.AreEqual(inventory.FullSlots, 0, 0);
			Assert.AreEqual(inventory.FreeSlots, 0, 0);
			Assert.Throws<IndexOutOfRangeException>(() => { ItemStack asd = inventory[0]; });
		}
	}
}