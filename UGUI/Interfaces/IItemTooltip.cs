using UnityEngine.EventSystems;

namespace Kryz.RPG.Items.UGUI
{
	public interface IItemTooltip
	{
		void Show(ItemStack itemStack, PointerEventData eventData);
		void Hide();
	}
}