using Cappta.Logging.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Cappta.Logging.Services
{
	public class RetryLogService : ILogService
	{
		private readonly ILogService logService;
		private readonly TimeSpan timeout;

		public RetryLogService(ILogService logService, TimeSpan timeout)
		{
			this.logService = logService ?? throw new ArgumentNullException(nameof(logService));
			this.timeout = timeout;
		}

		public void Log(IDictionary<string, object> data)
			=> this.Log(new JsonLog(data));

		public void Log(JsonLog jsonLog)
		{
			var stopwatch = Stopwatch.StartNew();

			while (true)
			{
				try
				{
					this.logService.Log(jsonLog);
					return;
				}
				catch when (stopwatch.Elapsed < this.timeout) { /* Do nothing */ }
			}
		}
	}
}
