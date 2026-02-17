using System;
using System.Buffers;
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

		public void FromItems(IItemFactory itemFactory, ReadOnlySpan<ItemStack> items)
		{
			ItemStacks.EnsureCapacity(items.Length);
			ItemStacks.Clear();

			for (int i = 0; i < items.Length; i++)
			{
				ItemStack stack = items[i];

				if (stack.Item != null && stack.Count > 0)
				{
					ItemStacks.Add(new ItemStackSaveData(itemFactory.ToSaveData(stack.Item), stack.Count));
				}
				else
				{
					ItemStacks.Add(default);
				}
			}
		}

		public void ToItemContainer(IItemFactory itemFactory, IItemContainer inventory)
		{
			ItemStack[] stacksArray = ArrayPool<ItemStack>.Shared.Rent(ItemStacks.Count);
			Span<ItemStack> stacks = stacksArray.AsSpan(0, ItemStacks.Count);

			for (int i = 0; i < stacks.Length; i++)
			{
				ItemStackSaveData saveData = ItemStacks[i];

				if (saveData.Item != null && saveData.Count > 0)
				{
					stacks[i] = new ItemStack(itemFactory.CreateFromSave(saveData.Item), saveData.Count);
				}
			}

			inventory.CopyFrom(stacks);

			stacks.Clear();
			ArrayPool<ItemStack>.Shared.Return(stacksArray);
		}
	}
}