﻿using System;
using System.Collections.Generic;

namespace Open.Disposable
{
	public interface IObjectPool<T> : IDisposable
		where T : class
	{
		/// <summary>
		/// Defines the maximum at which the pool can grow.
		/// </summary>
		int Capacity { get; }

		/// <summary>
		/// Directly calls the underlying factory that generates the items.  (No pool interaction.)
		/// </summary>
		T Generate();

		/// <summary>
		/// Receives an item and adds it to the pool. Ignores null references.
		/// WARNING: The item is considered 'dead' but resurrectable so be sure not to hold on to the item's reference.
		/// </summary>
		/// <param name="item">The item to give up to the pool.</param>
		/// <param name="recycler">An optional action exectue on the item only if it's possible to return to the pool.</param>
		void Give(T item);

		/// <summary>
		/// If the pool has an item currently avaialable, removes it from the pool and provides it as the out parameter.
		/// </summary>
		/// <param name="item">The item to return if available.  Will be null if none avaialable.</param>
		/// <returns>True if an item is provided.</returns>
		bool TryTake(out T item);

		/// <summary>
		/// If the pool has an item currently avaialable, removes it from the pool and returns it.
		/// </summary>
		/// <returns>The item to return if available.  Will be null if none avaialable.</returns>
		T TryTake();


		/// <summary>
		/// If the pool has an item currently avaialable, removes it from the pool and returns it.
		/// If none is available, it generates one.
		/// </summary>
		/// <returns>An item removed from the pool or generated.  Should never be null.</returns>
		T Take();

		
	}

	public static partial class ObjectPoolExtensions
	{
		/// <summary>
		/// Receives items and iteratively adds them to the pool.
		/// WARNING: These items are considered 'dead' but resurrectable so be sure not to hold on to their reference.
		/// </summary>
		/// <param name="items">The items to give up to the pool.</param>
		public static void Give<T>(this IObjectPool<T> target, IEnumerable<T> items)
			where T : class
		{
			if (items != null)
				foreach (var i in items)
					target.Give(i);
		}

		/// <summary>
		/// Receives items and iteratively adds them to the pool.
		/// WARNING: These items are considered 'dead' but resurrectable so be sure not to hold on to their reference.
		/// </summary>
		/// <param name="item2">The first item to give up to the pool.</param>
		/// <param name="item2">The second item to give up to the pool.</param>
		/// <param name="items">The remaining items to give up to the pool.</param>
		public static void Give<T>(this IObjectPool<T> target, T item1, T item2, params T[] items)
			where T : class
		{
			target.Give(item1);
			target.Give(item2);
			target.Give(items);
		}
		
	}
}
