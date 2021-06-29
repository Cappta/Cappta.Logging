using System;

namespace Cappta.Logging.Models {
	internal class Scope : IDisposable {
		private readonly Action<Scope> disposeAction;

		public Scope(object content, Action<Scope> disposeAction) {
			this.Content = content;
			this.disposeAction = disposeAction;
		}

		public object Content { get; }

		public void Dispose()
			=> this.disposeAction(this);
	}
}
