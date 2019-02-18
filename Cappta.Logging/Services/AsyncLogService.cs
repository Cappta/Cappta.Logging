using Cappta.Logging.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Cappta.Logging.Services
{
	public class AsyncLogService : ILogService, IDisposable
	{
		private const int DEFAULT_BATCH_LIMIT = 10;
		private const int DEFAULT_SIZE_LIMIT = 10000;

		private static readonly TimeSpan IDLE_SLEEP_TIME = TimeSpan.FromMilliseconds(100);

		public Action<Exception> Exception;
		public Action<int> SizeLimitReached;

		private readonly ConcurrentQueue<JsonLog> jsonLogQueue = new ConcurrentQueue<JsonLog>();
		private readonly ILogService logService;
		private readonly int batchLimit;
		private readonly int sizeLimit;

		private int lostLogCounter = 0;
		private bool disposing = false;

		public AsyncLogService(ILogService logService, int batchLimit = DEFAULT_BATCH_LIMIT, int sizeLimit = DEFAULT_SIZE_LIMIT)
		{
			this.logService = logService ?? throw new ArgumentNullException(nameof(logService));
			this.batchLimit = batchLimit;
			this.sizeLimit = sizeLimit;

			var thread = new Thread(this.IndexingThreadFunc);
			thread.Name = $"{nameof(AsyncLogService)}({logService.GetType().Name})"; //this helps identify this thread when debugging
			thread.IsBackground = true; //this means that the program can close with this thread running
			thread.Start();
		}

		public void Log(IDictionary<string, object> data)
			=> this.Log(new JsonLog(data));

		public void Log(IEnumerable<JsonLog> jsonLogs)
		{
			foreach (var jsonLog in jsonLogs) { this.Log(jsonLog); }
		}

		public void Log(JsonLog jsonLog)
		{
			if (this.jsonLogQueue.Count > this.sizeLimit)
			{
				lock (this.SizeLimitReached)
				{
					this.lostLogCounter++;
					this.SizeLimitReached?.Invoke(this.lostLogCounter);
				}
			}
			this.jsonLogQueue.Enqueue(jsonLog);
		}

		private void IndexingThreadFunc()
		{
			while (this.disposing == false)
			{
				var jsonLogBatch = this.EnumerateNextJsonLogBatch().ToArray();

				if (jsonLogBatch.Length == 0) { Thread.Sleep(IDLE_SLEEP_TIME); continue; }

				try
				{
					this.logService.Log(jsonLogBatch);
				}
				catch (Exception ex)
				{
					this.Log(jsonLogBatch);
					this.Exception?.Invoke(ex);
				}
			}
		}

		private IEnumerable<JsonLog> EnumerateNextJsonLogBatch()
		{
			for (var i = 0; i < this.batchLimit; i++)
			{
				if (this.jsonLogQueue.TryDequeue(out var jsonLog) == false) { yield break; }
				yield return jsonLog;
			}
		}

		public void Dispose()
			=> this.disposing = true;
	}
}
