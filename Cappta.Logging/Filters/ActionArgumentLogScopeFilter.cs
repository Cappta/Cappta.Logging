using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cappta.Logging.Filters
{
	public class ActionArgumentLogScopeFilter : IAsyncActionFilter
	{
		private readonly ILogger<ActionArgumentLogScopeFilter> logger;

		public ActionArgumentLogScopeFilter(ILogger<ActionArgumentLogScopeFilter> logger)
			=> this.logger = logger ?? throw new System.ArgumentNullException(nameof(logger));

		public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			using (this.logger.BeginScope(new SortedDictionary<string, object>() { { "ActionArgument", context.ActionArguments } }))
			{
				await next();
			}
		}
	}
}
