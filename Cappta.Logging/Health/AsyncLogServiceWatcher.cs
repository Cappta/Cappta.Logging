using Cappta.Logging.Services;

namespace Cappta.Logging.Health {
	public class AsyncLogServiceWatcher : IAsyncLogServiceWatcher {
		private readonly AsyncLogService asyncLogService;

		public AsyncLogServiceWatcher(AsyncLogService asyncLogService) {
			this.asyncLogService = asyncLogService;
		}

		public int LostLogCount => this.asyncLogService.LostLogCount;
		public int QueueCapacity => this.asyncLogService.QueueCapacity;
		public int QueueCount => this.asyncLogService.QueueCount;
		public int RetryQueueCount => this.asyncLogService.RetryQueueCount;
		public bool ServiceRunning => this.asyncLogService.ServiceRunning;
	}
}
