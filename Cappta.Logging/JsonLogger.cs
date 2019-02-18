using Cappta.Logging.Converters;
using Cappta.Logging.Extensions;
using Cappta.Logging.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cappta.Logging
{
	internal class JsonLogger : ILogger
	{
		private readonly string categoryName;
		private readonly ILogConverter logConverter;
		private readonly ILogService logService;
		private readonly ScopeContainer scopeContainer;

		public JsonLogger(string categoryName, ILogConverter logConverter, ILogService logService, ScopeContainer scopeContainer)
		{
			if (string.IsNullOrEmpty(categoryName)) { throw new ArgumentNullException(nameof(categoryName)); }

			this.categoryName = categoryName;
			this.logConverter = logConverter ?? throw new ArgumentNullException(nameof(logConverter));
			this.logService = logService ?? throw new ArgumentNullException(nameof(logService));
			this.scopeContainer = scopeContainer ?? throw new ArgumentNullException(nameof(scopeContainer));
		}

		public IDisposable BeginScope<TState>(TState state)
			=> this.scopeContainer.BeginScope(state);

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
			log.MergeWith(this.ObjectToDict(state));

			var objectScopes = this.scopeContainer.ObjectScopes;
			log.MergeWith(objectScopes.Select(objectScope => this.ObjectToDict(objectScope.State)).ToArray());

			log.RemoveNullValues();
			var flatLog = log.Flatten();
			this.logService.Log(flatLog);
		}

		private IDictionary<string, object> ObjectToDict(object obj)
		{
			var logObject = this.logConverter.ConvertToLogObject(obj);
			return logObject is IDictionary<string, object> dictionary
				? dictionary
				: new SortedDictionary<string, object>() { { obj.GetType().Name, obj } };
		}
	}
}
