using RestSharp;
using System;

namespace Cappta.Logging.Models.Exceptions {
	public class ApiResponseException : Exception {
		public ApiResponseException(RestResponse restResponse)
			: base($"Received status {(int)restResponse.StatusCode} with \"{restResponse.Content}\"")
			=> this.Response = restResponse;

		public RestResponse Response { get; }
	}
}
