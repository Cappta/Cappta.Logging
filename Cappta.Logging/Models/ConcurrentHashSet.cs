using System;
using System.Collections.Generic;

namespace Cappta.Logging.Models
{
	public class ConcurrentHashSet<T>
	{
		private readonly object lockObject = new object();
		private readonly HashSet<T> hashSet = new HashSet<T>();

		public void Add(T item)
			=> this.Locking(() => this.hashSet.Add(item));

		public void Remove(T item)
			=> this.Locking(() => this.hashSet.Remove(item));

		public int Count
			=> this.Locking(() => this.hashSet.Count);

		private TResult Locking<TResult>(Func<TResult> func)
		{
			lock (this.lockObject)
			{
				return func();
			}
		}
	}
}
