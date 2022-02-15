using Cappta.Logging.Converters;
using Cappta.Logging.Extensions;
using Cappta.Logging.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Cappta.Logging {
	internal class JsonLogger : ILogger {
		private readonly string categoryName;
		private readonly ILogConverterFactory logConverterFactory;
		private readonly ILogService logService;
		private readonly IExternalScopeProvider scopeProvider;
		private readonly ISecretProvider secretProvider;

		public JsonLogger(
			string categoryName,
			ILogConverterFactory logConverterFactory,
			ILogService logService,
			IExternalScopeProvider scopeProvider,
			ISecretProvider secretProvider
		) {
			this.categoryName = categoryName;
			this.logConverterFactory = logConverterFactory;
			this.logService = logService;
			this.scopeProvider = scopeProvider;
			this.secretProvider = secretProvider;
		}

		public IDisposable BeginScope<TState>(TState state)
			=> this.scopeProvider.Push(state);

		public bool IsEnabled(LogLevel logLevel) => true; //Do not block logs from here

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) {
			var scopeSecretProvider = new SecretProvider();
			var logConverter = this.logConverterFactory.Create(scopeSecretProvider);

			var log = new SortedDictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
				{
					{ "Category", this.categoryName },
					{ "Event", logConverter.ConvertToLogObject(eventId) },
					{ "LogLevel", logLevel },
					{ "Exception", logConverter.ConvertToLogObject(exception) }
				};

			this.scopeProvider.ForEachScope(
					(scope, dict) => this.MergeScopes(logConverter, scope, dict),
					log
				);

			log.MergeWith(this.ObjectToDict(logConverter, state));

			log.RemoveNullValues();
			var flatLog = log.Flatten();

			scopeSecretProvider.Protect(flatLog);
			this.secretProvider.Protect(flatLog);

			this.logService.Log(flatLog);
		}

		private void MergeScopes(ILogConverter logConverter, object? scope, IDictionary<string, object?> dict)
			=> dict.MergeWith(this.ObjectToDict(logConverter, scope));

		private IDictionary<string, object?> ObjectToDict(ILogConverter logConverter, object? obj) {
			var logObject = logConverter.ConvertToLogObject(obj);

			if(logObject is IDictionary<string, object?> dict) { return dict; }
			if(obj is null) { return new SortedDictionary<string, object?>(StringComparer.OrdinalIgnoreCase); }

			return new SortedDictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { { obj.GetType().Name, obj } };
		}
	}
}
