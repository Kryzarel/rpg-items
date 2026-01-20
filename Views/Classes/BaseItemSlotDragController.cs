using UnityEngine;

namespace Kryz.RPG.Items.Views
{
	public abstract class BaseItemSlotDragController : MonoBehaviour, IItemSlotDragController
	{
		public abstract IItemSlotView? CursorSlot { get; }
		public abstract IItemSlotView? SourceSlot { get; }

		public abstract bool StartDrag(IItemSlotView slotView, int count);
		public abstract bool StopDrag();
	}
}