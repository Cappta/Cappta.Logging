using Cappta.Logging;
using Cappta.Logging.Converters;
using Cappta.Logging.Extensions;
using Cappta.Logging.Serializer;
using Cappta.Logging.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Sample
{
	class Program
	{
		static void Main(string[] args)
		{
			var serviceProvider = new ServiceCollection()
				.AddLogging(loggingBuilder => loggingBuilder.AddProvider(JsonLoggerProvider.Instance))
				.AddSharedScopeContainer()
				.BuildServiceProvider();

#if true //ElasticSearch
			ScopeContainer.Global.BeginScope(new Dictionary<string, object>() { { "Host", Environment.MachineName } });
			var apiLogService = new ElasticSearchLogService(@"http://scorpion-homolog.cappta.com.br:9200", "garbage", JsonSerializer.Instance);
#elif true //SPLUNK
			ScopeContainer.Global.BeginScope(new Dictionary<string, object>() { { "ProviderName", "PaymentTransactionAuditor" }, { "Host", Environment.MachineName } });
			var apiLogService = new SplunkService(@"https://splunk.cappta.com.br:8088", "afecfb05-856f-42ef-a5f7-81d8342bf11f", JsonSerializer.Instance);
#elif true //GRAYLOG
			ScopeContainer.Global.BeginScope(new Dictionary<string, object>() { { "stream-key", "garbage" }, { "Host", Environment.MachineName } });
			var apiLogService = new GrayLogService(@"http://50shades.cappta.com.br:12201", JsonSerializer.Instance);
#endif

			var logConverter = new LogConverter();
			var asyncLogService = new AsyncLogService(apiLogService);
			JsonLoggerProvider.Instance.Configure(logConverter, asyncLogService, serviceProvider);

			var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
			using (logger.BeginScope(new { Operation = "PerformAction" }))
			{
				var user = "Arroz";
				using (logger.BeginScope("User {User} has logged in", user))
				{
					var action = "Cooking Rice";
					try
					{
						var logger2 = serviceProvider.GetRequiredService<ILogger<JsonLoggerProvider>>();
						logger2.LogInformation(new EventId(1, "GenericInfo"), "User is {Action}", action);
						Step1();
					}
					catch (Exception ex)
					{
						logger.LogError(new EventId(3, "GenericError"), ex, "Something is not right when {Action}", action);
					}
				}
			}
			Thread.Sleep(TimeSpan.FromSeconds(10));
		}

		private static void Step1()
			=> Step2();

		private static void Step2()
			=> throw new InvalidOperationException("The treta has been planted");
	}
}
