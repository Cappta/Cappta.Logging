using Cappta.Logging.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Cappta.Logging.Health {
	public class AsyncLogHealthCheck : IHealthCheck {
		private static readonly int ACCEPTABLE_QUEUE_COUNT_DIVISOR = 10;

		private readonly IServiceProvider serviceProvider;

		public AsyncLogHealthCheck(IServiceProvider serviceProvider)
			=> this.serviceProvider = serviceProvider;

		public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
			=> await Task.FromResult(this.CheckHealth());

		private HealthCheckResult CheckHealth() {
			var asyncLogServiceWatcher = this.serviceProvider.GetService<IAsyncLogServiceWatcher>();
			if(asyncLogServiceWatcher == null) { return HealthCheckResult.Unhealthy($"{nameof(IAsyncLogServiceWatcher)} has not been registered into IOC"); }

			var pendingRetryCount = asyncLogServiceWatcher.PendingRetryLogCount;
			var queueCount = asyncLogServiceWatcher.QueueCount;
			var acceptableQueueCount = asyncLogServiceWatcher.QueueCapacity / ACCEPTABLE_QUEUE_COUNT_DIVISOR;

			if(asyncLogServiceWatcher.ServiceRunning is false) {
				return HealthCheckResult.Unhealthy(
					$"Async Indexing Service is not running",
					data: this.GetResultData(asyncLogServiceWatcher, acceptableQueueCount)
				);
			}

			if(pendingRetryCount > 0) {
				return HealthCheckResult.Unhealthy(
					$"We're failing to index some logs and they may be lost beyond recover",
					data: this.GetResultData(asyncLogServiceWatcher, acceptableQueueCount)
				);
			}

			if(queueCount > acceptableQueueCount) {
				return HealthCheckResult.Unhealthy(
					$"The queue is growing faster than the indexing rate, we're on the way to lose logs beyond recover",
					data: this.GetResultData(asyncLogServiceWatcher, acceptableQueueCount)
				);
			}

			return HealthCheckResult.Healthy(
				$"Everything is fine",
				data: this.GetResultData(asyncLogServiceWatcher, acceptableQueueCount)
			);
		}

		private SortedDictionary<string, object> GetResultData(IAsyncLogServiceWatcher asyncLogServiceWatcher, int acceptableQueueCount)
			=> new()
			{
				{ nameof(acceptableQueueCount).ToPascalCase(), acceptableQueueCount },
				{ nameof(asyncLogServiceWatcher.LostLogCount), asyncLogServiceWatcher.LostLogCount },
				{ nameof(asyncLogServiceWatcher.QueueCapacity), asyncLogServiceWatcher.QueueCapacity },
				{ nameof(asyncLogServiceWatcher.QueueCount), asyncLogServiceWatcher.QueueCount },
				{ nameof(asyncLogServiceWatcher.RetryQueueCount), asyncLogServiceWatcher.RetryQueueCount },
				{ nameof(asyncLogServiceWatcher.PendingRetryLogCount), asyncLogServiceWatcher.PendingRetryLogCount },
			};
	}
}
