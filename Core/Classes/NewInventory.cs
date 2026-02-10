namespace Kryz.RPG.Items
{
	public sealed class NewInventory : ItemContainer<StandardTransactions>
	{
		public NewInventory(int size) : base(new StandardTransactions(size))
		{
		}
	}
}