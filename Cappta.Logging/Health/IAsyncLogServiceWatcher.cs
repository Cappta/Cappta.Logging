using System.Collections.ObjectModel;

namespace Cappta.Logging.Health
{
	public interface IAsyncLogServiceWatcher
	{
		int BusyIndexerCount { get; }
		ReadOnlyDictionary<string, int> ExceptionMessageCountDictionary { get; }
		int HealthyIndexerCount { get; }
		int LostLogCount { get; }
		int QueueCapacity { get; }
		int QueueCount { get; }
		int RetryQueueCount { get; }
		int PendingRetryCount { get; }
	}
}
