using Cappta.Logging.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Cappta.Logging.Services
{
	public class AsyncLogService : ILogService, IDisposable
	{
		private const int DEFAULT_SYNC_JOBS = 10;
		private const int DEFAULT_SIZE_LIMIT = 10000;

		private static readonly TimeSpan IDLE_SLEEP_TIME = TimeSpan.FromMilliseconds(100);

		public Action<Exception> Exception;
		public Action<int> SizeLimitReached;

		private readonly ConcurrentQueue<JsonLog> jsonLogQueue = new ConcurrentQueue<JsonLog>();
		private readonly ILogService logService;
		private readonly int sizeLimit;

		private int lostLogCounter = 0;
		private bool disposing = false;

		public AsyncLogService(ILogService logService, int syncJobs = DEFAULT_SYNC_JOBS, int sizeLimit = DEFAULT_SIZE_LIMIT)
		{
			this.logService = logService ?? throw new ArgumentNullException(nameof(logService));
			this.sizeLimit = sizeLimit;

			this.CreateSyncThreads(syncJobs);
		}

		private void CreateSyncThreads(int syncJobs)
		{
			for (var i = 0; i < syncJobs; i++)
			{
				var thread = new Thread(this.IndexingThreadFunc);
				thread.Name = $"{nameof(AsyncLogService)}({this.logService.GetType().Name}) #{i}"; //this helps identify this thread when debugging
				thread.IsBackground = true; //this means that the program can close with this thread running
				thread.Start();
			}
		}

		public void Log(IDictionary<string, object> data)
			=> this.Log(new JsonLog(data));

		public void Log(JsonLog jsonLog)
		{
			if (this.jsonLogQueue.Count > this.sizeLimit)
			{
				lock (this.SizeLimitReached)
				{
					this.lostLogCounter++;
					this.SizeLimitReached?.Invoke(this.lostLogCounter);
					return;
				}
			}
			this.jsonLogQueue.Enqueue(jsonLog);
		}

		private void IndexingThreadFunc()
		{
			while (this.disposing == false)
			{
				var hasJsonLog = this.jsonLogQueue.TryDequeue(out var jsonLog);

				if (hasJsonLog == false) { Thread.Sleep(IDLE_SLEEP_TIME); continue; }

				try
				{
					this.logService.Log(jsonLog);
				}
				catch (Exception ex)
				{
					this.Log(jsonLog);
					this.Exception?.Invoke(ex);
				}
			}
		}

		public void Dispose()
			=> this.disposing = true;
	}
}
