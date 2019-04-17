using Cappta.Logging.Converters;
using Cappta.Logging.Services;
using Microsoft.Extensions.Logging;
using System;

namespace Cappta.Logging
{
	public class JsonLoggerProvider : ILoggerProvider, ISupportExternalScope
	{
		private readonly ILogConverter logConverter;
		private readonly ILogService logService;
		private IExternalScopeProvider scopeProvider;

		public JsonLoggerProvider(ILogConverter logConverter, ILogService logService)
		{
			this.logConverter = logConverter ?? throw new ArgumentNullException(nameof(logConverter));
			this.logService = logService ?? throw new ArgumentNullException(nameof(logService));
		}

		public ILogger CreateLogger(string categoryName)
			=> new JsonLogger(categoryName, this.logConverter, this.logService, this.scopeProvider);

		public void Dispose() { }

		public void SetScopeProvider(IExternalScopeProvider scopeProvider)
			=> this.scopeProvider = scopeProvider;
	}
}
