﻿using Cappta.Logging.Models;
using Cappta.Logging.Models.Exceptions;
using Cappta.Logging.Serializer;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Cappta.Logging.Services
{
	public class SplunkService : ILogService
	{
		private const string LOG_ENDPOINT = @"services/collector";
		private const string AUTHORIZATION_HEADER = "Authorization";
		private const string AUTHORIZATION_HEADER_VALUE_PREFIX = "Splunk  ";

		private readonly RestClient restClient;
		private readonly string authorizationHeaderValue;
		private readonly ISerializer serializer;

		public SplunkService(string splunkUri, string token, ISerializer serializer)
		{
			if (string.IsNullOrWhiteSpace(splunkUri)) { throw new ArgumentNullException(nameof(splunkUri)); }
			if (string.IsNullOrWhiteSpace(token)) { throw new ArgumentNullException(nameof(token)); }

			this.restClient = new RestClient(splunkUri);
			this.authorizationHeaderValue = AUTHORIZATION_HEADER_VALUE_PREFIX + token;
			this.serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
		}

		public JsonSerializerSettings JsonSerializerSettings { get; }

		private string Host => Environment.MachineName;

		public void Log(IDictionary<string, object> data) => this.Log(new JsonLog(data));

		public void Log(JsonLog jsonLog) => this.Log(new JsonLog[] { jsonLog });

		public void Log(IEnumerable<JsonLog> jsonLogs)
		{
			var restRequest = new RestRequest(LOG_ENDPOINT, Method.POST);
			restRequest.AddHeader(AUTHORIZATION_HEADER, this.authorizationHeaderValue);
			var json = string.Join(Environment.NewLine, jsonLogs
					.Select(jsonLog => new SplunkHecRequest(this.Host, jsonLog))
					.Select(splunkHecRequest => this.serializer.Serialize(splunkHecRequest))
				);
			restRequest.AddJsonBody(json);

			var response = this.restClient.Execute(restRequest);

			if (response.IsSuccessful == true) { return; }

			throw response.ErrorException ?? new ApiResponseException(response.StatusCode);
		}
	}
}