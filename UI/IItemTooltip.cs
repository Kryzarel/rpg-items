using Kryz.RPG.Items.Structs;
using UnityEngine.EventSystems;

namespace Kryz.RPG.Items.UI
{
	public interface IItemTooltip
	{
		void Show(ItemStack itemStack, PointerEventData eventData);
		void Hide();
	}
}