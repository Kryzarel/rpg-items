namespace Kryz.RPG.Items
{
	public readonly struct SizeEdit
	{
		public readonly IItemContainer Container;
		public readonly int OldSize;
		public readonly int NewSize;

		public SizeEdit(IItemContainer container, int oldSize, int newSize)
		{
			Container = container;
			OldSize = oldSize;
			NewSize = newSize;
		}
	}
}