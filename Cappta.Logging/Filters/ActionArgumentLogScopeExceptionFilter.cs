using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cappta.Logging.Filters
{
	public abstract class ActionArgumentLogScopeExceptionFilter : IExceptionFilter, IAsyncActionFilter
	{
		private IDictionary<string, object> actionArguments;

		private readonly ILogger<ActionArgumentLogScopeExceptionFilter> logger;

		public ActionArgumentLogScopeExceptionFilter(ILogger<ActionArgumentLogScopeExceptionFilter> logger)
			=> this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

		public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			this.actionArguments = context.ActionArguments;
			await next();
		}

		public void OnException(ExceptionContext context)
		{
			var state = new SortedDictionary<string, object>() {
				{ "ActionArgument", this.actionArguments }
			};
			using (this.logger.BeginScope(state))
			{
				context.Result = this.Handle(context.Exception);
			}
		}

		protected abstract IActionResult Handle(Exception ex);
	}
}
