using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cappta.Logging.Filters
{
	public class AuthorizedClientIdLogScopeFilter : IAsyncActionFilter
	{
		private readonly ILogger<AuthorizedClientIdLogScopeFilter> logger;

		public AuthorizedClientIdLogScopeFilter(ILogger<AuthorizedClientIdLogScopeFilter> logger)
			=> this.logger = logger;

		public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			var claimsPrincipal = context.HttpContext.User;
			var clientIdClaim = claimsPrincipal?.FindFirst("client_id");
			var clientId = clientIdClaim?.Value;

			var state = new SortedDictionary<string, object?>(StringComparer.OrdinalIgnoreCase) {
				{ "AuthorizedClientId", clientId }
			};
			using (this.logger.BeginScope(state))
			{
				await next();
			}
		}
	}
}
