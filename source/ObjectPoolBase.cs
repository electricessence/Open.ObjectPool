using System;
using System.Runtime.CompilerServices;

namespace Open.Disposable;

public abstract class ObjectPoolBase<T>
	: ObjectPoolSimpleBase<T>
	where T : class
{
	protected ObjectPoolBase(
		Func<T> factory,
		Action<T>? recycler,
		Action<T>? disposer,
		int capacity = DEFAULT_CAPACITY)
		: base(factory, recycler, disposer, capacity)
	{ }

	protected override sealed int PocketCount => Pocket.Value is null ? 0 : 1;

	// ReSharper disable once UnassignedField.Global
	protected ReferenceContainer<T> Pocket; // Default struct constructs itself.

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected override bool SaveToPocket(T item) => Pocket.TrySave(item);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected override T? TakeFromPocket() => Pocket.TryRetrieve();
}
