using Cappta.Logging.Converters;
using Cappta.Logging.Services;
using Microsoft.Extensions.Logging;

namespace Cappta.Logging
{
	public class JsonLoggerProvider : ILoggerProvider, ISupportExternalScope
	{
		private readonly ScopeProvider scopeProvider = new ScopeProvider();

		private readonly ILogConverterFactory logConverterFactory;
		private readonly ILogService logService;

		public JsonLoggerProvider(ILogConverterFactory logConverterFactory, ILogService logService)
		{
			this.logConverterFactory = logConverterFactory;
			this.logService = logService;
		}

		public IScopeProvider ScopeProvider => this.scopeProvider;

		public ILogger CreateLogger(string categoryName)
			=> new JsonLogger(categoryName, this.logConverterFactory, this.logService, this.scopeProvider);

		public void Dispose() { }

		public void SetScopeProvider(IExternalScopeProvider externalScopeProvider)
			=> this.scopeProvider.ExternalScopeProvider = externalScopeProvider;
	}
}
