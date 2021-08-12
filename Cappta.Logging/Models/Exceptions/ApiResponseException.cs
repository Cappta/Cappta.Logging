using Cappta.Logging.Converters;
using RestSharp;
using System;
using System.Collections.Generic;

namespace Cappta.Logging.Models.Exceptions {
	public class ApiResponseException : Exception, ILogConvertable {
		public ApiResponseException(IRestResponse restResponse)
			: base($"Received status {(int)restResponse.StatusCode} with \"{restResponse.Content}\"")
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
