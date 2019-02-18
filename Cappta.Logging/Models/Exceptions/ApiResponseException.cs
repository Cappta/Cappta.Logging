using Cappta.Logging.Converters;
using System;
using System.Collections.Generic;
using System.Net;

namespace Cappta.Logging.Models.Exceptions
{
	public class ApiResponseException : Exception, ILogConvertable
	{
		public ApiResponseException(HttpStatusCode httpStatusCode)
			=> this.HttpStatusCode = httpStatusCode;

		public ApiResponseException(HttpStatusCode httpStatusCode, string message)
			: base(message)
			=> this.HttpStatusCode = httpStatusCode;

		public ApiResponseException(HttpStatusCode httpStatusCode, string message, Exception innerException)
			: base(message, innerException)
			=> this.HttpStatusCode = httpStatusCode;

		public HttpStatusCode HttpStatusCode { get; }

		public object Convert(ILogConverter logSerializer)
			=> new SortedDictionary<string, object>() {
				{ "HttpStatusCode", this.HttpStatusCode },
				{ "InnerException", logSerializer.ConvertToLogObject(this.InnerException) },
				{ "Message", this.Message },
				{ "StackTrace", this.StackTrace },
				{ "Type", this.GetType().FullName },
			};
	}
}
