namespace Kryz.RPG.Items
{
	public sealed class NewInventory : ItemContainerBase<ItemContainerTransactional>
	{
		public NewInventory(int size) : base(size)
		{
		}
	}
}