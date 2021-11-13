#define ElasticSearch
//#define Splunk
//#define GrayLog

using Cappta.Logging;
using Cappta.Logging.Converters;
using Cappta.Logging.Serializer;
using Cappta.Logging.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sample {
	class Program {
		static async Task Main(string[] args) {
#if ElasticSearch
			var apiLogService = new ElasticSearchLogService(@"http://scorpion-homolog.cappta.com.br:9200", "garbage", new JsonSerializer());
#elif Splunk
			var apiLogService = new SplunkService(@"https://splunk.cappta.com.br:8088", "afecfb05-856f-42ef-a5f7-81d8342bf11f", new JsonSerializer());
#elif GrayLog
			var apiLogService = new GrayLogService(@"http://50shades.cappta.com.br:12201", new JsonSerializer());
#endif

			var asyncLogService = new AsyncLogService(apiLogService);
			var jsonLoggerProvider = new JsonLoggerProvider(new LogConverterFactory(), asyncLogService);

			await asyncLogService.StartAsync(default);

			var serviceProvider = new ServiceCollection()
				.AddLogging(loggingBuilder => loggingBuilder.AddProvider(jsonLoggerProvider))
				.BuildServiceProvider(true);

			var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
			logger.BeginScope("Running under {Host}", Environment.MachineName);
#if Splunk
			logger.BeginScope(new { ProviderName = "PaymentTransactionAuditor" });
#elif GrayLog
			logger.BeginScope(new { stream-key = "garbage" });
#endif

			var scopedServiceProvider = serviceProvider.CreateScope().ServiceProvider;
			DoSomething(scopedServiceProvider);

			var scopedServiceProvider2 = serviceProvider.CreateScope().ServiceProvider;
			DoSomethingElse(scopedServiceProvider2);

			await asyncLogService.StopAsync(default);
		}

		private static void DoSomething(IServiceProvider serviceProvider) {
			var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
			using(logger.BeginScope(new { Operation = nameof(DoSomething) })) {
				using(logger.BeginScope("User {User} has logged in", "Arroz")) {
					var action = "Cooking Rice";
					try {
						var logger2 = serviceProvider.GetRequiredService<ILogger<JsonLoggerProvider>>();
						logger2.LogInformation(new EventId(1, "GenericInfo"), "User is {Action}", action);
						Step1();
					} catch(Exception ex) {
						logger.LogError(new EventId(3, "GenericError"), ex, "Something is not right when {Action}", action);
					}
				}
			}
		}

		private static void DoSomethingElse(IServiceProvider serviceProvider) {
			var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
			using(logger.BeginScope(new { Operation = nameof(DoSomethingElse) })) {
				using(logger.BeginScope(new SampleObject() { Data = "Data", Secret = "Secret" })) {
					using(logger.BeginScope("Target {Target} has been found", "Dog")) {
						try {
							var logger2 = serviceProvider.GetRequiredService<ILogger<JsonLoggerProvider>>();
							logger2.LogInformation(new EventId(1, "GenericInfo"), "Sucessful {Procedure}", "Clean up");
							Step1();
						} catch(Exception ex) {
							logger.LogError(new EventId(3, "GenericError"), ex);
						}
					}
				}
			}
		}

		private static void Step1()
			=> Step2();

		private static void Step2()
			=> throw new InvalidOperationException("The treta has been planted");
	}
}
