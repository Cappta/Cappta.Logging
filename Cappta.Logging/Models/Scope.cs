using System;

namespace Cappta.Logging.Models
{
	internal class Scope : IDisposable
	{
		private readonly Action<Scope> disposeAction;

		public Scope(object content, Action<Scope> disposeAction)
		{
			this.Content = content ?? throw new ArgumentNullException(nameof(content));
			this.disposeAction = disposeAction ?? throw new ArgumentNullException(nameof(disposeAction));
		}

		public object Content { get; }

		public void Dispose()
			=> this.disposeAction(this);
	}
}
