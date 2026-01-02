using UnityEngine.EventSystems;

namespace Kryz.RPG.Items.Views
{
	public interface IItemTooltip
	{
		void Show(ItemStack itemStack, PointerEventData eventData);
		void Hide();
	}
}