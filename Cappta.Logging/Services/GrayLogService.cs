using Cappta.Logging.Extensions;
using Cappta.Logging.Models;
using Cappta.Logging.Models.Exceptions;
using Cappta.Logging.Serializer;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cappta.Logging.Services
{
	public class GrayLogService : ILogService
	{
		private const string URN = "gelf";

		private readonly RestClient restClient;
		private readonly ISerializer serializer;

		public GrayLogService(string grayLogUri, ISerializer serializer)
		{
			if (string.IsNullOrWhiteSpace(grayLogUri)) { throw new ArgumentNullException(nameof(grayLogUri)); }

			this.restClient = new RestClient(grayLogUri);
			this.serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
		}

		public void Log(JsonLog jsonLog)
			=> this.Log(jsonLog.Data);

		public void Log(IDictionary<string, object?> data)
		{
			var camelCaseData = data.ToDictionary(kvp => kvp.Key.ToCamelCase(), kvp => kvp.Value);

			var request = new RestRequest(URN, Method.POST);

			var json = this.serializer.Serialize(camelCaseData);
			request.AddRawJsonBody(json);

			var response = this.restClient.Execute(request);
			if (response.IsSuccessful == true) { return; }

			throw response.ErrorException ?? new ApiResponseException(response.StatusCode);
		}
	}
}
