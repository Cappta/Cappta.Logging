using System.Collections.ObjectModel;

namespace Cappta.Logging.Health {
	public interface IAsyncLogServiceWatcher {
		int LostLogCount { get; }
		int QueueCapacity { get; }
		int QueueCount { get; }
		int RetryQueueCount { get; }
		int PendingRetryLogCount { get; }
	}
}
