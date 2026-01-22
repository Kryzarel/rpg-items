using System.Diagnostics.CodeAnalysis;

namespace Kryz.RPG.Items
{
	public interface IItemDefinitionDatabase
	{
		IItemDefinition Get(string id);

		bool TryGet(string id, [MaybeNullWhen(returnValue: false)] out IItemDefinition definition);
	}
}