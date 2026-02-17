#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Kryz.RPG.Items.Views
{
	public partial class ItemContainerView : MonoBehaviour
	{
		partial void OnValidateEditor()
		{
			if (!EditorApplication.isPlayingOrWillChangePlaymode)
			{
				EditorApplication.delayCall += HandleSlotViewsEditor;
			}
		}

		private void HandleSlotViewsEditor()
		{
			int maxSlots = this.maxSlots > 0 ? this.maxSlots : int.MaxValue;
			int numSlots = Math.Min(Math.Max(0, minSlots), maxSlots);

			for (int i = itemSlots.Count; i < numSlots; i++)
			{
				BaseItemSlotView itemSlotView = (BaseItemSlotView)PrefabUtility.InstantiatePrefab(itemSlotPrefab, itemSlotsParent);
				itemSlots.Add(itemSlotView);
				itemSlotView.Unbind();
				itemSlotView.gameObject.SetActive(true);
			}

			for (int i = itemSlots.Count - 1; i >= numSlots; i--)
			{
				DestroyImmediate(itemSlots[i].gameObject);
				itemSlots.RemoveAt(i);
			}
		}
	}
}
#endif