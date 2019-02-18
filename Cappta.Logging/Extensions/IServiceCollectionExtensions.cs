using Microsoft.Extensions.DependencyInjection;

namespace Cappta.Logging.Extensions
{
	public static class IServiceCollectionExtensions
	{
		public static IServiceCollection AddSharedScopeContainer(this IServiceCollection serviceCollection)
			=> serviceCollection.AddScoped<ScopeContainer>();
	}
}
