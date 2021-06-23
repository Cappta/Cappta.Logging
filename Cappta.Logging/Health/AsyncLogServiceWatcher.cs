using Cappta.Logging.Models;
using Cappta.Logging.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Cappta.Logging.Health {
	public class AsyncLogServiceWatcher : IAsyncLogServiceWatcher {
		private readonly AsyncLogService asyncLogService;
		private readonly Dictionary<string, int> exceptionMessageCountDict = new();
		private readonly ConcurrentHashSet<int> failingHashSet = new();

		public AsyncLogServiceWatcher(AsyncLogService asyncLogService) {
			this.asyncLogService = asyncLogService;
			this.asyncLogService.Success += this.OnAsyncLogServiceSuccess;
			this.asyncLogService.Exception += this.OnAsyncLogServiceException;
		}

		public int BusyIndexerCount => this.asyncLogService.BusyIndexerCount;
		public ReadOnlyDictionary<string, int> ExceptionMessageCountDictionary {
			get {
				lock(this.exceptionMessageCountDict) {
					return new ReadOnlyDictionary<string, int>(this.exceptionMessageCountDict);
				}
			}
		}
		public int HealthyIndexerCount => this.asyncLogService.HealthyIndexerCount;
		public int LostLogCount => this.asyncLogService.LostLogCount;
		public int QueueCapacity => this.asyncLogService.QueueCapacity;
		public int QueueCount => this.asyncLogService.QueueCount;
		public int RetryQueueCount => this.asyncLogService.RetryQueueCount;
		public int PendingRetryCount => this.failingHashSet.Count;

		private void OnAsyncLogServiceSuccess(int hashCode)
			=> this.failingHashSet.Remove(hashCode);

		private void OnAsyncLogServiceException(int hashCode, Exception ex) {
			this.failingHashSet.Add(hashCode);
			lock(this.exceptionMessageCountDict) {
				if(this.exceptionMessageCountDict.ContainsKey(ex.Message)) {
					this.exceptionMessageCountDict[ex.Message]++;
					return;
				}

				this.exceptionMessageCountDict.Add(ex.Message, 1);
			}
		}
	}
}