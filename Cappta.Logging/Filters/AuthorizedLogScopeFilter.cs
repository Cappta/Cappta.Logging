using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cappta.Logging.Filters {
	public class AuthorizedLogScopeFilter : IAsyncActionFilter {
		private readonly ILogger<AuthorizedLogScopeFilter> logger;

		public AuthorizedLogScopeFilter(ILogger<AuthorizedLogScopeFilter> logger)
			=> this.logger = logger;

		public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next) {
			var claimsPrincipal = context.HttpContext.User;
			var clientId = claimsPrincipal.FindFirst("client_id")?.Value;
			var userName = claimsPrincipal.FindFirst("username")?.Value;

			var state = new SortedDictionary<string, object?>(StringComparer.OrdinalIgnoreCase) {
				{ "AuthorizedClientId", clientId },
				{ "AuthorizedUserName", userName }
			};
			using(this.logger.BeginScope(state)) {
				await next();
			}
		}
	}
}