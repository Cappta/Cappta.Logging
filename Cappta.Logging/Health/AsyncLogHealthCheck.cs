using Cappta.Logging.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Cappta.Logging.Health
{
	public class AsyncLogHealthCheck : IHealthCheck
	{
		private static readonly int ACCEPTABLE_QUEUE_COUNT_DIVISOR = 10;

		private readonly IServiceProvider serviceProvider;

		public AsyncLogHealthCheck(IServiceProvider serviceProvider)
			=> this.serviceProvider = serviceProvider;

		public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
			=> await Task.FromResult(this.CheckHealth());

		private HealthCheckResult CheckHealth()
		{
			var asyncLogServiceWatcher = this.serviceProvider.GetService<IAsyncLogServiceWatcher>();
			if (asyncLogServiceWatcher == null) { return HealthCheckResult.Unhealthy($"{nameof(IAsyncLogServiceWatcher)} has not been registered into IOC"); }

			var lostLogCounter = asyncLogServiceWatcher.LostLogCount;
			var queueCount = asyncLogServiceWatcher.QueueCount;
			var acceptableQueueCount = asyncLogServiceWatcher.QueueCapacity / ACCEPTABLE_QUEUE_COUNT_DIVISOR;

			if (lostLogCounter > 0)
			{
				return HealthCheckResult.Unhealthy(
					$"{lostLogCounter} logs have been lost already and the queue is currently {queueCount} entries long",
					data: this.GetResultData(asyncLogServiceWatcher, acceptableQueueCount)
				);
			}

			if (queueCount > acceptableQueueCount)
			{
				return HealthCheckResult.Degraded(
					$"No logs have been lost yet but the queue is currently {queueCount} entries long, which is higher than the expected maximum of {acceptableQueueCount}",
					data: this.GetResultData(asyncLogServiceWatcher, acceptableQueueCount)
				);
			}

			return HealthCheckResult.Healthy(
				$"No logs have been lost yet and the queue is currently {queueCount} entries long",
				data: this.GetResultData(asyncLogServiceWatcher, acceptableQueueCount)
			);
		}

		private Dictionary<string, object> GetResultData(IAsyncLogServiceWatcher asyncLogServiceWatcher, int acceptableQueueCount)
			=> new Dictionary<string, object>()
			{
				{nameof(acceptableQueueCount).ToPascalCase(), acceptableQueueCount },
				{nameof(asyncLogServiceWatcher.QueueCount), asyncLogServiceWatcher.QueueCount },
				{nameof(asyncLogServiceWatcher.QueueCapacity), asyncLogServiceWatcher.QueueCapacity },
				{nameof(asyncLogServiceWatcher.LostLogCount), asyncLogServiceWatcher.LostLogCount },
				{nameof(asyncLogServiceWatcher.HealthyIndexerCount), asyncLogServiceWatcher.HealthyIndexerCount },
				{nameof(asyncLogServiceWatcher.BusyIndexerCount), asyncLogServiceWatcher.BusyIndexerCount },
				{nameof(asyncLogServiceWatcher.ExceptionMessageCountDictionary), asyncLogServiceWatcher.ExceptionMessageCountDictionary },
			};
	}
}
