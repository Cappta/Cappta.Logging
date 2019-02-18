using System;
using System.Collections.Generic;
using System.Linq;

namespace Cappta.Logging
{
	public class ScopeContainer
	{
		private readonly List<ObjectScope> scopes = new List<ObjectScope>();

		public static ScopeContainer Global { get; } = new ScopeContainer();

		internal IEnumerable<ObjectScope> ObjectScopes
		{
			get
			{
				lock (this.scopes)
				{
					this.scopes.RemoveAll(scope => scope.Disposed);

					return this == Global
						? this.scopes.ToArray()
						: Global.ObjectScopes.Union(this.scopes);
				}
			}
		}

		public IDisposable BeginScope<TState>(TState state)
		{
			var objectScope = new ObjectScope(state);
			this.scopes.Add(objectScope);
			return objectScope;
		}
	}
}
