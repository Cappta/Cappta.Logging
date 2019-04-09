using Cappta.Logging.Converters;
using Cappta.Logging.Extensions;
using Cappta.Logging.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Cappta.Logging
{
	internal class JsonLogger : ILogger
	{
		private readonly string categoryName;
		private readonly ILogConverter logConverter;
		private readonly ILogService logService;
		private readonly IExternalScopeProvider scopeProvider;

		public JsonLogger(string categoryName, ILogConverter logConverter, ILogService logService, IExternalScopeProvider scopeProvider)
		{
			if (string.IsNullOrEmpty(categoryName)) { throw new ArgumentNullException(nameof(categoryName)); }

			this.categoryName = categoryName;
			this.logConverter = logConverter ?? throw new ArgumentNullException(nameof(logConverter));
			this.logService = logService ?? throw new ArgumentNullException(nameof(logService));
			this.scopeProvider = scopeProvider;
		}

		public IDisposable BeginScope<TState>(TState state)
			=> this.scopeProvider.Push(state);

		public bool IsEnabled(LogLevel logLevel) => true; //Do not block logs from here

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			var log = new SortedDictionary<string, object>()
				{
					{ "Category", this.categoryName },
					{ "Event", this.logConverter.ConvertToLogObject(eventId) },
					{ "LogLevel", logLevel },
					{ "Exception", this.logConverter.ConvertToLogObject(exception) }
				};

			this.scopeProvider.ForEachScope(this.MergeScopes, log);
			log.MergeWith(this.ObjectToDict(state));

			log.RemoveNullValues();
			var flatLog = log.Flatten();
			this.logService.Log(flatLog);
		}

		private void MergeScopes(object scope, IDictionary<string, object> dict)
			=> dict.MergeWith(this.ObjectToDict(scope));

		private IDictionary<string, object> ObjectToDict(object obj)
		{
			var logObject = this.logConverter.ConvertToLogObject(obj);
			return logObject is IDictionary<string, object> dictionary
				? dictionary
				: new SortedDictionary<string, object>() { { obj.GetType().Name, obj } };
		}
	}
}
