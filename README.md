# Cappta-Logging
SDK responsável por abstrair a entrega dos logs gerados pela sua aplicação em .Net Core para Splunk ou Graylog no formato JSON.

## Uso do Software
Referencie o projeto como submodulo em seu projeto para possibilitar debugging quando necessario.

Adicione a instância do JsonLoggerProvider ao como Provider do LoggingBuilder
Em ConfigureLogging para IWebHostBuilder ou AddLogging para IServiceCollection
```csharp
ConfigureLogging(loggingBuilder => loggingBuilder.AddProvider(JsonLoggerProvider.Instance))
AddLogging(loggingBuilder => loggingBuilder.AddProvider(JsonLoggerProvider.Instance))
```

Utilize o ScopeContainer.Global.BeginScope para adicionar um escopo em todos os logs da aplicação.
Recomenda-se utilizar Dictionary<string, object> para adicionar dados de chave e valor, por ser mais performatico na operação.

Para possibilitar o compartilhamento de escopo entre resoluções de dependencia dentro do mesmo escopo, utilize o método de extensão AddSharedScopeContainer no seu ServiceCollection.
```csharp
public void ConfigureServices(IServiceCollection services)
	=> services.AddSharedScopeContainer()
```

Configure o JsonLoggerProvider com o método Configure.

[Para mais informações veja como utilizar o logging to Asp.Net Core.](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-2.2)

Código de exemplo:
```csharp
var serviceProvider = new ServiceCollection()
		.AddLogging(loggingBuilder => loggingBuilder.AddProvider(JsonLoggerProvider.Instance))
		.AddSharedScopeContainer()
		.BuildServiceProvider();

#if GRAYLOG
	ScopeContainer.Global.BeginScope(new Dictionary<string, object>() { { "stream-key", "garbage" }, { "Host", Environment.MachineName } });
	var apiLogService = new GrayLogService(@"http://50shades.cappta.com.br:12201", JsonSerializer.Instance);
#elif SPLUNK
	ScopeContainer.Global.BeginScope(new Dictionary<string, object>() { { "ProviderName", "PaymentTransactionAuditor" }, { "Host", Environment.MachineName } });
	var apiLogService = new SplunkService(@"https://splunk.cappta.com.br:8088", "afecfb05-856f-42ef-a5f7-81d8342bf11f", JsonSerializer.Instance);
#endif

var logConverter = new LogConverter();
var asyncLogService = new AsyncLogService(apiLogService);
JsonLoggerProvider.Instance.Configure(logConverter, asyncLogService);

var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
using (logger.BeginScope(new { Operation = "PerformAction" }))
{
	var user = "Arroz";
	using (logger.BeginScope("User {User} has logged in", user))
	{
		var action = "Cooking Rice";
		try
		{
			var logger2 = serviceProvider.GetRequiredService<ILogger<Robot>>();
			logger.LogInformation(new EventId(1, "GenericInfo"), "User is {Action}", action);
			Step1();
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Something is not right when {Action}", action);
		}
	}
}
Thread.Sleep(Timeout.Infinite);
```

## Ferramentas
1. C#
   1. Dapper
   1. Microsoft.Extensions.DependencyInjection
   1. Microsoft.Extensions.Logging
   1. Newtonsoft.Json
   1. RestSharp
 
 ## Colaboradores
 - Sérgio Fonseca - _Autor inicial_ - SammyROCK
