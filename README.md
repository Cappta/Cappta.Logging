# Cappta-Logging
SDK responsável por abstrair a entrega dos logs gerados pela sua aplicação em .Net Core para Splunk, Graylog ou ElasticSearch no formato JSON.

## Uso do Software
Referencie o projeto como submodulo em seu projeto para possibilitar debugging quando necessario.

Adicione a instância do JsonLoggerProvider como Provider do LoggingBuilder através do método ConfigureLogging para IWebHostBuilder ou AddLogging para IServiceCollection
```csharp
var jsonLoggerProvider = new JsonLoggerProvider(logConverter, logService);
ConfigureLogging(loggingBuilder => loggingBuilder.AddProvider(jsonLoggerProvider))
AddLogging(loggingBuilder => loggingBuilder.AddProvider(jsonLoggerProvider))
```

Para adicionar um escopo em todos os logs da aplicação, utilize um logger no root do IServiceProvider.   
Caso a performance seja relevante recomenda-se utilizar Dictionary<string, object> para adicionar dados de chave e valor, por ser mais performatico ao não utilizar reflection para extrair os campos.   
Exemplo:
```csharp
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
	var logger = app.ApplicationServices.GetService<ILogger<Startup>>();
	logger.BeginScope(new { Host = Environment.MachineName });
```

Para adicionar automaticamente os argumentos de um end point Asp.Net Core MVC, adicione o filtro ActionArgumentLogScopeFilter ao adicionar o MVC ao IServiceCollection.   
Exemplo:
```csharp
public void ConfigureServices(IServiceCollection services)
	=> services.AddMvc(options => options.Filters.Add<ActionArgumentLogScopeFilter>());
```

Para adicionar o tratamento de exceção global dos end points Asp.Net Core MVC com suporte a objetos do escopo da requisição e com os argumentos do end point adicionado no log, extenda a classe abstrata ActionArgumentLogScopeExceptionFilter e adicione-o ao adicionar o MVC ao IServiceCollection, conforme explicado anteriormente.   

[Para mais informações veja como utilizar o logging to Asp.Net Core.](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-2.2)

[Entenda melhor com um código de exemplo em um Console Application.](https://github.com/Cappta/Cappta-Logging/blob/master/Sample/Program.cs)

## Ferramentas
1. C#
   1. Dapper
   1. Microsoft.Extensions.DependencyInjection
   1. Microsoft.Extensions.Logging
   1. Microsoft.NETCore.App
   1. Newtonsoft.Json
   1. RestSharp
 
 ## Colaboradores
 - Sérgio Fonseca - _Autor inicial_ - SammyROCK
