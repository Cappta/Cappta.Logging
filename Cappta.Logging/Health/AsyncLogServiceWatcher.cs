using Cappta.Logging.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Cappta.Logging.Health
{
	public class AsyncLogServiceWatcher : IAsyncLogServiceWatcher
	{
		private readonly AsyncLogService asyncLogService;
		private readonly Dictionary<string, int> exceptionMessageCountDict = new Dictionary<string, int>();

		public AsyncLogServiceWatcher(AsyncLogService asyncLogService)
		{
			this.asyncLogService = asyncLogService;
			this.asyncLogService.Exception += this.OnAsyncLogServiceException;
		}

		public int BusyIndexerCount => this.asyncLogService.BusyIndexerCount;
		public ReadOnlyDictionary<string, int> ExceptionMessageCountDictionary
		{
			get
			{
				lock (this.exceptionMessageCountDict)
				{
					return new ReadOnlyDictionary<string, int>(this.exceptionMessageCountDict);
				}
			}
		}
		public int HealthyIndexerCount => this.asyncLogService.HealthyIndexerCount;
		public int LostLogCount => this.asyncLogService.LostLogCount;
		public int QueueCapacity => this.asyncLogService.QueueCapacity;
		public int QueueCount => this.asyncLogService.QueueCount;

		private void OnAsyncLogServiceException(Exception ex)
		{
			lock (this.exceptionMessageCountDict)
			{
				if (this.exceptionMessageCountDict.ContainsKey(ex.Message))
				{
					this.exceptionMessageCountDict[ex.Message]++;
					return;
				}

				this.exceptionMessageCountDict.Add(ex.Message, 1);
			}
		}
	}
}
