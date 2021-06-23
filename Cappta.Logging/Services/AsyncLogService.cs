using Cappta.Logging.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Cappta.Logging.Services {
	public class AsyncLogService : ILogService, IDisposable {
		private const int DEFAULT_SYNC_JOBS = 10;
		private const int DEFAULT_SIZE_LIMIT = 10000;

		private static readonly TimeSpan IDLE_SLEEP_TIME = TimeSpan.FromMilliseconds(100);

		public event Action<int>? Success;
		public event Action<int, Exception>? Exception;

		private readonly ConcurrentQueue<JsonLog> retryJsonLogQueue = new();
		private readonly ConcurrentQueue<JsonLog> mainJsonLogQueue = new();
		private readonly ILogService logService;

		private int busyIndexerCount = 0;
		private int healthyIndexerCount = 0;
		private int lostLogCount = 0;
		private bool disposing = false;

		public AsyncLogService(ILogService logService, int syncJobs = DEFAULT_SYNC_JOBS, int queueCapacity = DEFAULT_SIZE_LIMIT) {
			this.logService = logService;
			this.QueueCapacity = queueCapacity;

			this.CreateSyncThreads(syncJobs);
		}

		public int BusyIndexerCount => this.busyIndexerCount;
		public int HealthyIndexerCount => this.healthyIndexerCount;
		public int LostLogCount => this.lostLogCount;
		public int QueueCapacity { get; }
		public int QueueCount => this.mainJsonLogQueue.Count + this.RetryQueueCount;
		public int RetryQueueCount => this.retryJsonLogQueue.Count;

		private void CreateSyncThreads(int syncJobs) {
			for(var i = 0; i < syncJobs; i++) {
				var thread = new Thread(this.IndexingThreadFunc);
				thread.Name = $"{nameof(AsyncLogService)}({this.logService.GetType().Name}) #{i}"; //this helps identify this thread when debugging
				thread.IsBackground = true; //this means that the program can close with this thread running
				thread.Priority = ThreadPriority.Lowest; //Make sure we don't interfere with actual behavior of the application
				thread.Start();
			}
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

		private void IndexingThreadFunc() {
			try {
				Interlocked.Increment(ref this.healthyIndexerCount);
				Interlocked.Increment(ref this.busyIndexerCount);
				while(this.disposing == false) {
					var hasJsonLog = this.mainJsonLogQueue.TryDequeue(out var jsonLog)
						|| this.retryJsonLogQueue.TryDequeue(out jsonLog);

					if(hasJsonLog == false) {
						Interlocked.Decrement(ref this.busyIndexerCount);
						Thread.Sleep(IDLE_SLEEP_TIME);
						Interlocked.Increment(ref this.busyIndexerCount);
						continue;
					}

					try {
						this.logService.Log(jsonLog);

						try { this.Success?.Invoke(jsonLog.GetHashCode()); } catch { /* Ignore */ }
					} catch(Exception ex) {
						this.Log(jsonLog);

						try { this.Exception?.Invoke(jsonLog.GetHashCode(), ex); } catch { /* Ignore */ }
					}
				}
			} finally {
				Interlocked.Decrement(ref this.healthyIndexerCount);
				Interlocked.Decrement(ref this.busyIndexerCount);
			}
		}

		public void Dispose()
			=> this.disposing = true;
	}
}