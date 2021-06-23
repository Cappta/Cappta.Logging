using Cappta.Logging.Converters;
using RestSharp;
using System;
using System.Collections.Generic;

namespace Cappta.Logging.Models.Exceptions {
	public class ApiResponseException : Exception, ILogConvertable {
		public ApiResponseException(IRestResponse restResponse)
			=> this.Response = restResponse;

		public ApiResponseException(IRestResponse restResponse, string message)
			: base(message)
			=> this.Response = restResponse;

		public ApiResponseException(IRestResponse restResponse, string message, Exception innerException)
			: base(message, innerException)
			=> this.Response = restResponse;

		public IRestResponse Response { get; }

		public object Convert(ILogConverter logSerializer)
			=> new SortedDictionary<string, object?>(StringComparer.OrdinalIgnoreCase) {
				{ "Response", logSerializer.ConvertToLogObject(this.Response) },
				{ "InnerException", logSerializer.ConvertToLogObject(this.InnerException) },
				{ "Message", this.Message },
				{ "StackTrace", this.StackTrace },
				{ "Type", this.GetType().FullName },
			};
	}
}