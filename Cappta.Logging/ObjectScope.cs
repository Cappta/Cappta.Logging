using System;

namespace Cappta.Logging
{
	internal class ObjectScope : IDisposable
	{
		public ObjectScope(object state)
			=> this.State = state ?? throw new ArgumentNullException(nameof(state));

		public bool Disposed { get; private set; }
		public object State { get; }

		public void Dispose()
			=> this.Disposed = true;
	}
}
