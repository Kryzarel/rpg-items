namespace Kryz.RPG.Items
{
	public readonly struct SizeEdit
	{
		public readonly int OldSize;
		public readonly int NewSize;

		public SizeEdit(int oldSize, int newSize)
		{
			OldSize = oldSize;
			NewSize = newSize;
		}
	}
}