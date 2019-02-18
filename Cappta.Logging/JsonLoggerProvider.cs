using Cappta.Logging.Converters;
using Cappta.Logging.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using ScopeContainerClass = Cappta.Logging.ScopeContainer;

namespace Cappta.Logging
{
	public class JsonLoggerProvider : ILoggerProvider
	{
		public static JsonLoggerProvider Instance { get; } = new JsonLoggerProvider();

		private bool configured = false;
		private ILogConverter logConverter;
		private ILogService logService;
		private IServiceProvider serviceProvider;

		public void Configure(ILogConverter logConverter, ILogService logService)
		{
			if (this.configured) { throw new InvalidOperationException($"Cannot configure {nameof(JsonLoggerProvider)} again"); }

			this.logConverter = logConverter ?? throw new ArgumentNullException(nameof(logConverter));
			this.logService = logService ?? throw new ArgumentNullException(nameof(logService));
			this.configured = true;
		}

		private ScopeContainerClass ScopeContainer
			=> this.serviceProvider?.GetService<ScopeContainerClass>() ?? ScopeContainerClass.Global;

		public void SetServiceProvider(IServiceProvider serviceProvider)
			=> this.serviceProvider = serviceProvider;

		public ILogger CreateLogger(string categoryName)
		{
			if (this.configured == false) { throw new InvalidOperationException($"{nameof(JsonLoggerProvider)} has not been configured yet"); }

			return new JsonLogger(categoryName, this.logConverter, this.logService, this.ScopeContainer);
		}

		public void Dispose() { }
	}
}
