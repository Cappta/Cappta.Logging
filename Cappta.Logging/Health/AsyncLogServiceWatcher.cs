using Cappta.Logging.Models;
using Cappta.Logging.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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
		public int PendingRetryLogCount => this.asyncLogService.PendingRetryLogCount;
	}
}
