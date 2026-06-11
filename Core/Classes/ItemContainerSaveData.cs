using System;
using System.Collections.Generic;
using Kryz.Utils;
using UnityEngine;

namespace Kryz.RPG.Items
{
	[Serializable]
	public class ItemContainerSaveData
	{
		[SerializeField]
		public List<ItemStackSaveData> ItemStacks = new();

		public void FromItemContainer(IItemFactory itemFactory, IReadOnlyItemContainer container)
		{
			ReadOnlySpan<ItemStack> items = container.Slots;

			ItemStacks.Clear();
			ItemStacks.EnsureCapacity(items.Length);

			for (int i = 0; i < items.Length; i++)
			{
				ItemStack stack = items[i];
				ItemStackSaveData saveData = default;

				if (stack.Item != null && stack.Count > 0)
				{
					saveData = new ItemStackSaveData(itemFactory.ToSaveData(stack.Item), stack.Count);
				}

				ItemStacks.Add(saveData);
			}
		}

		public void ToItemContainer(IItemFactory itemFactory, IItemContainer container)
		{
			using ItemTransaction transaction = new(container);

			container.SetSize(transaction, ItemStacks.Count);

			for (int i = 0; i < ItemStacks.Count; i++)
			{
				ItemStackSaveData saveData = ItemStacks[i];
				ItemStack stack = default;

				if (saveData.Item != null && saveData.Count > 0)
				{
					stack = new ItemStack(itemFactory.CreateFromSave(saveData.Item), saveData.Count);
				}

				container.SetSlot(transaction, i, stack);
			}

			transaction.SetResult(true);
		}
	}
}