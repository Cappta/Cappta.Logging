using System;

namespace Cappta.Logging.Models {
	internal class Disposable : IDisposable {
		private readonly Action disposeAction;

		public Disposable(Action disposeAction) {
			this.disposeAction = disposeAction;
		}

		public void Dispose()
			=> this.disposeAction();
	}
}
