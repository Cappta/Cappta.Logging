using Cappta.Logging.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Cappta.Logging.Services {
	public class AsyncLogService : ILogService, IDisposable {
		private const int DEFAULT_SIZE_LIMIT = 10000;

		private static readonly TimeSpan IDLE_SLEEP_TIME = TimeSpan.FromSeconds(1);

		private readonly ConcurrentQueue<JsonLog> mainJsonLogQueue = new();
		private readonly ConcurrentQueue<JsonLog> retryJsonLogQueue = new();
		private readonly IBatchableLogService logService;

		private int lostLogCount = 0;
		private bool disposing = false;

		public AsyncLogService(IBatchableLogService logService, int queueCapacity = DEFAULT_SIZE_LIMIT) {
			this.logService = logService;
			this.QueueCapacity = queueCapacity;

			this.CreateSyncThread();
		}

		public int LostLogCount => this.lostLogCount;
		public int QueueCapacity { get; }
		public int QueueCount => this.mainJsonLogQueue.Count + this.RetryQueueCount;
		public int PendingRetryLogCount { get; private set; }
		public int RetryQueueCount => this.retryJsonLogQueue.Count;
		public int BatchSize { get; set; } = 50;

		private void CreateSyncThread() {
			var thread = new Thread(() => { this.IndexingThreadFunc(); });
			thread.Name = $"{nameof(AsyncLogService)}({this.logService.GetType().Name})"; //this helps identify this thread when debugging
			thread.IsBackground = true; //this means that the program can close with this thread running
			thread.Priority = ThreadPriority.Lowest; //Make sure we don't interfere with actual behavior of the application
			thread.Start();
		}

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

		private void IndexingThreadFunc() {
			while(this.disposing == false) {
				if(this.QueueCount is 0) {
					Thread.Sleep(IDLE_SLEEP_TIME);
					continue;
				}

				var batch = this.EnumerateJsonLogs().Take(this.BatchSize).ToArray();
				try {
					this.logService.Log(batch, this.OnLogFailed);
					this.PendingRetryLogCount = this.RetryQueueCount;
				} catch {
					this.OnLogFailed(batch);
					Thread.Sleep(IDLE_SLEEP_TIME);
				}
			}
		}

		private IEnumerable<JsonLog> EnumerateJsonLogs() {
			while(true) {
				var hasJsonLog = this.mainJsonLogQueue.TryDequeue(out var jsonLog)
					|| this.retryJsonLogQueue.TryDequeue(out jsonLog);

				if(hasJsonLog is false) { yield break; }

				yield return jsonLog;
			}
		}

		private void OnLogFailed(JsonLog[] jsonLogs) {
			foreach(var jsonLog in jsonLogs) {
				this.RetryLog(jsonLog);
			}
		}

		public void Dispose()
			=> this.disposing = true;
	}
}
