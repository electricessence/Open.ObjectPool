using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Open.Disposable;

public sealed class ThreadLocalQueueObjectPool<T>
	: ObjectPoolBase<T>
	where T : class
{
	public ThreadLocalQueueObjectPool(
		Func<T> factory,
		Action<T>? recycler,
		Action<T>? disposer,
		int capacity = DEFAULT_CAPACITY - 10)
		: base(factory, recycler, disposer, capacity)
	{
		Pool = new(() => new (capacity), true);
	}

	public ThreadLocalQueueObjectPool(
		Func<T> factory,
		int capacity = DEFAULT_CAPACITY - 10)
		: this(factory, null, null, capacity) { }

	readonly ThreadLocal<Queue<T>> Pool;

	public override int Count => Pool.Value!.Count;

	/*
	 * NOTE: ConcurrentQueue is very fast and will perform quite well without using the 'Pocket' feature.
	 * Benchmarking reveals that mixed read/writes (what really matters) are still faster with the pocket enabled so best to keep it so.
	 */

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected override bool Receive(T item)
	{
		Pool.Value!.Enqueue(item); // It's possible that the count could exceed MaxSize here, but the risk is negligble as a few over the limit won't hurt.
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected override T? TryRelease()
	{
		var p = Pool.Value!;
		Debug.Assert(p is not null);
#if NETSTANDARD2_0
		if(p.Count == 0) return null;
		var item = p.Dequeue();
#else
		p.TryDequeue(out var item);
#endif
		return item;
	}

	protected override void OnDispose()
	{
		base.OnDispose();
		Pool.Dispose();
	}
}

public static class ThreadLocalQueueObjectPool
{
	public static ThreadLocalQueueObjectPool<T> Create<T>(Func<T> factory, int capacity = Constants.DEFAULT_CAPACITY)
		where T : class => new(factory, capacity);

	public static ThreadLocalQueueObjectPool<T> Create<T>(int capacity = Constants.DEFAULT_CAPACITY)
		where T : class, new() => Create(() => new T(), capacity);

	public static ThreadLocalQueueObjectPool<T> CreateAutoRecycle<T>(Func<T> factory, int capacity = Constants.DEFAULT_CAPACITY)
		 where T : class, IRecyclable => new(factory, Recycler.Recycle, null, capacity);

	public static ThreadLocalQueueObjectPool<T> CreateAutoRecycle<T>(int capacity = Constants.DEFAULT_CAPACITY)
		where T : class, IRecyclable, new() => CreateAutoRecycle(() => new T(), capacity);

	public static ThreadLocalQueueObjectPool<T> CreateAutoDisposal<T>(Func<T> factory, int capacity = Constants.DEFAULT_CAPACITY)
		where T : class, IDisposable => new(factory, null, d => d.Dispose(), capacity);

	public static ThreadLocalQueueObjectPool<T> CreateAutoDisposal<T>(int capacity = Constants.DEFAULT_CAPACITY)
		where T : class, IDisposable, new() => CreateAutoDisposal(() => new T(), capacity);
}
