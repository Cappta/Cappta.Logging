using Cappta.Logging.Converters;
using Cappta.Logging.Services;
using Microsoft.Extensions.Logging;
using System;

namespace Cappta.Logging
{
	public class JsonLoggerProvider : ILoggerProvider, ISupportExternalScope
	{
		private bool configured = false;
		private ILogConverter logConverter;
		private ILogService logService;
		private IExternalScopeProvider scopeProvider;

		public static JsonLoggerProvider Instance { get; } = new JsonLoggerProvider();

		public void Configure(ILogConverter logConverter, ILogService logService)
		{
			if (this.configured) { throw new InvalidOperationException($"Cannot configure {nameof(JsonLoggerProvider)} again"); }

			this.logConverter = logConverter ?? throw new ArgumentNullException(nameof(logConverter));
			this.logService = logService ?? throw new ArgumentNullException(nameof(logService));
			this.configured = true;
		}

		public ILogger CreateLogger(string categoryName)
		{
			if (this.configured == false) { throw new InvalidOperationException($"{nameof(JsonLoggerProvider)} has not been configured yet"); }

			return new JsonLogger(categoryName, this.logConverter, this.logService, this.scopeProvider);
		}

		public void Dispose() { }

		public void SetScopeProvider(IExternalScopeProvider scopeProvider)
			=> this.scopeProvider = scopeProvider;
	}
}
