# Cappta-Logging
SDK responsável por abstrair a entrega dos logs gerados pela sua aplicação em .Net Core para Splunk ou Graylog no formato JSON.

## Uso do Software
Referencie o projeto como submodulo em seu projeto para possibilitar debugging quando necessario.

Adicione a instância do JsonLoggerProvider como Provider do LoggingBuilder através do método ConfigureLogging para IWebHostBuilder ou AddLogging para IServiceCollection
```csharp
ConfigureLogging(loggingBuilder => loggingBuilder.AddProvider(JsonLoggerProvider.Instance))
AddLogging(loggingBuilder => loggingBuilder.AddProvider(JsonLoggerProvider.Instance))
```

Utilize um logger no root do IServiceProvider para adicionar um escopo em todos os logs da aplicação.   
Recomenda-se utilizar Dictionary<string, object> para adicionar dados de chave e valor, por ser mais performatico na operação.   
Exemplo:
```csharp
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
	var logger = app.ApplicationServices.GetService<ILogger<Startup>>();
	logger.BeginScope(new { Host = Environment.MachineName });
```

Configure o JsonLoggerProvider com o método Configure.

[Para mais informações veja como utilizar o logging to Asp.Net Core.](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-2.2)

[Entenda melhor com um código de exemplo em um Console Application.](https://github.com/Cappta/Cappta-Logging/blob/master/Sample/Program.cs)

## Ferramentas
1. C#
   1. Dapper
   1. Microsoft.Extensions.DependencyInjection
   1. Microsoft.Extensions.Logging
   1. Newtonsoft.Json
   1. RestSharp
 
 ## Colaboradores
 - Sérgio Fonseca - _Autor inicial_ - SammyROCK
