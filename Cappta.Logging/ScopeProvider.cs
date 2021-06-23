using Cappta.Logging.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cappta.Logging {
	public class ScopeProvider : IScopeProvider, IExternalScopeProvider {
		private readonly List<Scope> scopes = new();

		internal IExternalScopeProvider? ExternalScopeProvider { get; set; }

		public void ForEachScope<TState>(Action<object, TState> callback, TState state) {
			foreach(var scopeContent in this.GetScopeContents()) {
				callback(scopeContent, state);
			}
			this.ExternalScopeProvider?.ForEachScope(callback, state);
		}

		private IEnumerable<object> GetScopeContents() {
			lock(this.scopes) {
				return this.scopes.Select(scope => scope.Content).ToArray();
			}
		}

		public IDisposable Push(object state) {
			var scope = new Scope(state, this.RemoveScope);
			lock(this.scopes) { this.scopes.Add(scope); }
			return scope;
		}

		private void RemoveScope(Scope scope) {
			lock(this.scopes) { this.scopes.Remove(scope); }
		}
	}
}