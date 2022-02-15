using Cappta.Logging.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Cappta.Logging {
	public class ScopeProvider : IScopeProvider, IExternalScopeProvider {
		private readonly List<object?> scopes = new();

		internal IExternalScopeProvider? ExternalScopeProvider { get; set; }

		public void ForEachScope<TState>(Action<object?, TState> callback, TState state) {
			foreach(var scopeContent in this.GetScopeContents()) {
				callback(scopeContent, state);
			}
			this.ExternalScopeProvider?.ForEachScope(callback, state);
		}

		private IEnumerable<object?> GetScopeContents() {
			lock(this.scopes) {
				return this.scopes.ToArray();
			}
		}

		public IDisposable Push(object? state) {
			var scope = new Disposable(() => this.RemoveScope(state));
			lock(this.scopes) { this.scopes.Add(state); }
			return scope;
		}

		private void RemoveScope(object? state) {
			lock(this.scopes) { this.scopes.Remove(state); }
		}
	}
}
