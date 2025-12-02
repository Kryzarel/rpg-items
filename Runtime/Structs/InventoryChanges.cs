using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace Kryz.RPG.Items.Structs
{
	public ref struct InventoryChanges
	{
		public readonly ReadOnlySpan<int> Indexes => indexes[..changesCount];
		public readonly ReadOnlySpan<ItemStack> NewSlots;

		private readonly ReadOnlySpan<ItemStack> currSlots;
		private readonly Span<int> indexes;
		private readonly ItemStack[] newSlots;
		private readonly ArrayPool<ItemStack> arrayPool;

		private int changesCount;

		public InventoryChanges(ReadOnlySpan<ItemStack> slots, Span<int> indexes, ArrayPool<ItemStack> arrayPool)
		{
			currSlots = slots;
			this.indexes = indexes;
			this.arrayPool = arrayPool;
			changesCount = 0;

			newSlots = arrayPool.Rent(slots.Length);
			Span<ItemStack> newSlotsSpan = newSlots[..slots.Length];
			slots.CopyTo(newSlotsSpan);
			NewSlots = newSlotsSpan;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void StoreChange(int index, ItemStack newStack)
		{
			ref ItemStack newSlot = ref newSlots[index];
			if (newSlot == currSlots[index]) // Only store indexes on the first change. If further changes are made we don't need another entry.
			{
				indexes[changesCount++] = index;
			}
			newSlot = newStack;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly void Dispose()
		{
			Array.Clear(newSlots, 0, NewSlots.Length);
			arrayPool.Return(newSlots);
		}
	}
}