using Cappta.Logging.Converters;
using Cappta.Logging.Services;
using Microsoft.Extensions.Logging;
using System;

namespace Cappta.Logging
{
	public class JsonLoggerProvider : ILoggerProvider, ISupportExternalScope
	{
		private readonly ScopeProvider scopeProvider = new ScopeProvider();

		private readonly ILogConverter logConverter;
		private readonly ILogService logService;

		public JsonLoggerProvider(ILogConverter logConverter, ILogService logService)
		{
			this.logConverter = logConverter ?? throw new ArgumentNullException(nameof(logConverter));
			this.logService = logService ?? throw new ArgumentNullException(nameof(logService));
		}

		public IScopeProvider ScopeProvider => this.scopeProvider;

		public ILogger CreateLogger(string categoryName)
			=> new JsonLogger(categoryName, this.logConverter, this.logService, this.scopeProvider);

		public void Dispose() { }

		public void SetScopeProvider(IExternalScopeProvider externalScopeProvider)
			=> this.scopeProvider.ExternalScopeProvider = externalScopeProvider;
	}
}
