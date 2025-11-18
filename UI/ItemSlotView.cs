using Kryz.RPG.Items.Structs;
using UnityEngine;
using UnityEngine.UI;

namespace Kryz.RPG.Items.UI
{
	public class ItemSlotView : MonoBehaviour
	{
		[SerializeField] Image image;
		[SerializeField] Text countText;

		private ItemStack itemStack;

		public void SetItemStack(ItemStack stack)
		{
			if (itemStack != stack)
			{
				if (image.sprite != stack.Item?.Definition.Icon)
				{
					image.sprite = stack.Item?.Definition.Icon!;
				}

				if (itemStack.Count != stack.Count)
				{
					countText.text = stack.Count.ToString();
				}

				itemStack = stack;
			}
		}
	}
}