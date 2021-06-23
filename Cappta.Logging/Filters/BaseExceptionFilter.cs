using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Cappta.Logging.Filters {
	public abstract class BaseExceptionFilter : IAsyncActionFilter {
		public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next) {
			var actionExecutedContext = await next();
			if(actionExecutedContext.Exception != null) {
				var handledResult = this.Handle(actionExecutedContext.Exception);
				if(handledResult is null) { return; }

				actionExecutedContext.Result = handledResult;
				actionExecutedContext.ExceptionHandled = true;
			}
		}

		protected abstract IActionResult? Handle(Exception ex);

		protected IActionResult InternalServerError(object? value = null)
			=> this.StatusCode(HttpStatusCode.InternalServerError, value);

		protected IActionResult StatusCode(HttpStatusCode httpStatusCode, object? value)
			=> new ObjectResult(value) { StatusCode = (int)httpStatusCode };
	}
}