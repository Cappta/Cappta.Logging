using Cappta.Logging.Models;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Cappta.Logging.Services {
	public class AsyncLogService : BackgroundService, ILogService {
		private const int DEFAULT_SIZE_LIMIT = 10000;

		private static readonly TimeSpan IDLE_SLEEP_TIME = TimeSpan.FromSeconds(1);
		private static readonly TimeSpan RETRY_COOLDOWN = TimeSpan.FromMinutes(1);

		private readonly ConcurrentQueue<JsonLog> mainJsonLogQueue = new();
		private readonly ConcurrentQueue<JsonLog> retryJsonLogQueue = new();
		private readonly IBatchableLogService logService;

		private int lostLogCount = 0;
		private DateTimeOffset lastRetryTime = DateTimeOffset.UtcNow;

		public AsyncLogService(IBatchableLogService logService, int queueCapacity = DEFAULT_SIZE_LIMIT) {
			this.logService = logService;
			this.QueueCapacity = queueCapacity;
		}

		public int BatchSize { get; set; } = 50;
		public int LostLogCount => this.lostLogCount;
		public int MainQueueCount => this.mainJsonLogQueue.Count;
		public int PendingRetryLogCount { get; private set; }
		public int QueueCapacity { get; }
		public int QueueCount => this.MainQueueCount + this.RetryQueueCount;
		public int RetryQueueCount => this.retryJsonLogQueue.Count;
		public bool ServiceRunning { get; private set; }

		public void Log(IDictionary<string, object?> data)
			=> this.Log(new JsonLog(data));

		public void Log(JsonLog jsonLog) {
			if(this.QueueCount > this.QueueCapacity) {
				Interlocked.Increment(ref this.lostLogCount);
				return;
			}
			this.mainJsonLogQueue.Enqueue(jsonLog);
		}

		private void RetryLog(JsonLog jsonLog) {
			if(this.QueueCount > this.QueueCapacity) {
				Interlocked.Increment(ref this.lostLogCount);
				return;
			}
			this.retryJsonLogQueue.Enqueue(jsonLog);
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
			try {
				this.ServiceRunning = true;

				while(stoppingToken.IsCancellationRequested is false || this.MainQueueCount > 0) {
					if(this.QueueCount is 0) {
						await Task.Delay(IDLE_SLEEP_TIME);
						continue;
					}

					await this.UploadBatch(stoppingToken);
				}

				var lostLogCount = this.RetryQueueCount + this.LostLogCount;
				if(lostLogCount is 0) {
					Console.WriteLine("Cappta-Logging: All logs were uploaded");
				} else {
					Console.WriteLine($"Cappta-Logging: Lost {lostLogCount} logs");
				}
			} catch(Exception ex) {
				Console.WriteLine($"Cappta-Logging: {nameof(AsyncLogService)}:{nameof(ExecuteAsync)} threw an exception: {ex}");
				await this.ExecuteAsync(stoppingToken);
			} finally {
				this.ServiceRunning = false;
			}
		}

		private async Task UploadBatch(CancellationToken stoppingToken) {
			var shouldIncludeRetries = stoppingToken.IsCancellationRequested is false &&
				DateTimeOffset.UtcNow - lastRetryTime > RETRY_COOLDOWN;
			var batch = this.EnumerateJsonLogs(shouldIncludeRetries).Take(this.BatchSize).ToArray();
			if(batch.Any() is false) {
				await Task.Delay(IDLE_SLEEP_TIME);
				return;
			}
			try {
				await this.logService.Log(batch, this.OnLogFailed);
				this.PendingRetryLogCount = this.RetryQueueCount;
			} catch {
				this.OnLogFailed(batch);
				await Task.Delay(IDLE_SLEEP_TIME);
			}
		}

		private IEnumerable<JsonLog> EnumerateJsonLogs(bool shouldIncludeRetries) {
			while(true) {
				var hasJsonLog = this.mainJsonLogQueue.TryDequeue(out var jsonLog);

				if(hasJsonLog is false && shouldIncludeRetries) {
					lastRetryTime = DateTimeOffset.UtcNow;
					hasJsonLog = this.retryJsonLogQueue.TryDequeue(out jsonLog);
				}

				if(hasJsonLog is false) { yield break; }

				yield return jsonLog;
			}
		}

		private void OnLogFailed(JsonLog[] jsonLogs) {
			foreach(var jsonLog in jsonLogs) {
				this.RetryLog(jsonLog);
			}
		}
	}
}
