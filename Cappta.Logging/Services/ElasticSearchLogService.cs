using Cappta.Logging.Extensions;
using Cappta.Logging.Models;
using Cappta.Logging.Models.Exceptions;
using Cappta.Logging.Serializer;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Cappta.Logging.Services
{
	public class ElasticSearchLogService : ILogService
	{
		private const string TIMESTAMP_FIELD = "@timestamp";
		private const string TIME_FORMAT = @"yyyy-MM-ddTHH:mm:ss.fffZ";

		private static readonly TimeSpan REQUEST_TIMEOUT = TimeSpan.FromSeconds(10);

		private readonly RestClient restClient;
		private readonly string resource;
		private readonly ISerializer serializer;

		public ElasticSearchLogService(string elasticSearchUri, string index, ISerializer serializer, string? token = null)
		{
			this.restClient = new RestClient(elasticSearchUri);
			if (string.IsNullOrEmpty(token) == false) { this.restClient.AddDefaultHeader("Authorization", $"Basic {token}"); }

			this.resource = $"{index}/default";
			this.serializer = serializer;
			this.restClient.Timeout = (int)REQUEST_TIMEOUT.TotalMilliseconds;
		}

		public void Log(IDictionary<string, object?> data)
			=> this.Log(new JsonLog(data));

		public void Log(JsonLog jsonLog)
		{
			var request = new RestRequest(this.resource, Method.POST);

			var utcLogTime = jsonLog.Time.ToUniversalTime();
			jsonLog.Data[TIMESTAMP_FIELD] = utcLogTime.ToString(TIME_FORMAT, CultureInfo.InvariantCulture);

			var json = this.serializer.Serialize(jsonLog.Data);
			request.AddRawJsonBody(json);

			var response = this.restClient.Execute(request);
			if (response.IsSuccessful == true) { return; }

			throw response.ErrorException ?? new ApiResponseException(response);
		}
	}
}
