using System;
using System.Buffers;
using UnityEngine;

namespace Kryz.RPG.Items
{
	[Serializable]
	public class InventorySaveData : ISerializationCallbackReceiver
	{
		[SerializeField]
		public ItemStackSaveData[] ItemStacks = Array.Empty<ItemStackSaveData>();
		[SerializeField]
		public int Count;

		public string ToJson()
		{
			return JsonUtility.ToJson(this, prettyPrint: true);
		}

		public void FromJson(string json)
		{
			JsonUtility.FromJsonOverwrite(json, this);
		}

		public void FromItems(ReadOnlySpan<ItemStack> items)
		{
			if (items.Length > ItemStacks.Length)
			{
				if (ItemStacks.Length > 0)
				{
					ArrayPool<ItemStackSaveData>.Shared.Return(ItemStacks, clearArray: true);
				}
				ItemStacks = ArrayPool<ItemStackSaveData>.Shared.Rent(items.Length);
			}

			Count = items.Length;

			for (int i = 0; i < items.Length; i++)
			{
				ItemStack stack = items[i];
				ItemStacks[i] = new ItemStackSaveData(stack.Item?.ToSaveData(), stack.Count);
			}
		}

		public void ToInventory(IItemFactory itemFactory, IInventory inventory)
		{
			ItemStack[] stacksArray = ArrayPool<ItemStack>.Shared.Rent(Count);
			Span<ItemStack> stacks = stacksArray.AsSpan(0, Count);
			Span<ItemStackSaveData> saveDatas = ItemStacks.AsSpan(0, Count);

			for (int i = 0; i < stacks.Length; i++)
			{
				ItemStackSaveData saveData = saveDatas[i];

				if (saveData.Item != null && saveData.Count > 0)
				{
					stacks[i] = new ItemStack(itemFactory.CreateFromSave(saveData.Item), saveData.Count);
				}
			}

			inventory.CopyFrom(stacks);

			stacks.Clear();
			ArrayPool<ItemStack>.Shared.Return(stacksArray);
		}

		public void OnAfterDeserialize()
		{
			Count = Math.Clamp(Count, 0, ItemStacks.Length);
		}

		public void OnBeforeSerialize()
		{
			Count = Math.Clamp(Count, 0, ItemStacks.Length);
		}
	}
}