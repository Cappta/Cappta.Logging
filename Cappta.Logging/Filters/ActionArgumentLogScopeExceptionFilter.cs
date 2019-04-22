using Cappta.Logging.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Cappta.Logging.Filters
{
	public abstract class ActionArgumentLogScopeExceptionFilter : IExceptionFilter
	{
		private readonly ILogger<ActionArgumentLogScopeExceptionFilter> logger;

		public ActionArgumentLogScopeExceptionFilter(ILogger<ActionArgumentLogScopeExceptionFilter> logger)
			=> this.logger = logger ?? throw new System.ArgumentNullException(nameof(logger));

		public void OnException(ExceptionContext context)
		{
			var state = new SortedDictionary<string, object>() {
				{ "ActionArgument", context.ModelState.ToRawValueDictionary() }
			};
			using (this.logger.BeginScope(state))
			{
				context.Result = this.Handle(context);
			}
		}

		protected abstract IActionResult Handle(ExceptionContext context);
	}
}
