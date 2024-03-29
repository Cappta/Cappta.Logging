namespace Cappta.Logging.Health {
	public interface IAsyncLogServiceWatcher {
		int LostLogCount { get; }
		int QueueCapacity { get; }
		int QueueCount { get; }
		int RetryQueueCount { get; }
		bool ServiceRunning { get; }
	}
}
