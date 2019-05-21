using System;

namespace Cappta.Logging
{
	public interface IScopeProvider
	{
		IDisposable Push(object state);
	}
}
