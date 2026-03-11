using UnityEngine;

namespace Kryz.RPG.Items.UGUI
{
	public abstract class BaseItemSlotDragController : MonoBehaviour, IItemSlotDragController
	{
		public abstract IItemSlotView? CursorSlot { get; }
		public abstract IItemSlotView? SourceSlot { get; }

		public abstract bool StartDrag(IItemSlotView slotView);
		public abstract bool StopDrag();
	}
}