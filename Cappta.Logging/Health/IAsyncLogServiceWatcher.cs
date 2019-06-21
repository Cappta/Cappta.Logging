using System.Collections.ObjectModel;

namespace Cappta.Logging.Health
{
	public interface IAsyncLogServiceWatcher
	{
		ReadOnlyDictionary<string, int> ExceptionMessageCountDictionary { get; }
		int LostLogCounter { get; }
		int QueueCapacity { get; }
		int QueueCount { get; }
	}
}
