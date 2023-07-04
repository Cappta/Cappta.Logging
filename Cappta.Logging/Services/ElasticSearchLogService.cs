using Cappta.Logging.Extensions;
using Cappta.Logging.Models;
using Cappta.Logging.Models.ElasticSearch;
using Cappta.Logging.Models.Exceptions;
using Cappta.Logging.Serializer;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Cappta.Logging.Services {
	public class ElasticSearchLogService : ILogService, IBatchableLogService {
		private const string TIMESTAMP_FIELD = "@timestamp";
		private const string TIME_FORMAT = @"yyyy-MM-ddTHH:mm:ss.fffZ";

		private static readonly TimeSpan REQUEST_TIMEOUT = TimeSpan.FromSeconds(10);

		private readonly RestClient restClient;
		private readonly ISerializer serializer;

		public ElasticSearchLogService(string elasticSearchUri, string index, ISerializer serializer, string? token = null) {
			this.restClient = new RestClient(new RestClientOptions() {
				MaxTimeout = (int)REQUEST_TIMEOUT.TotalMilliseconds,
				BaseUrl = new Uri(elasticSearchUri)
			});
			if(string.IsNullOrEmpty(token) == false) { this.restClient.AddDefaultHeader("Authorization", $"Basic {token}"); }

			this.Index = HttpUtility.UrlEncode(index);
			this.serializer = serializer;
		}

		public string Index { get; }

		public void Log(IDictionary<string, object?> data)
			=> this.Log(new JsonLog(data));

		public void Log(JsonLog jsonLog) {
			var restRequest = new RestRequest($"{this.Index}/default", Method.Post);

			var utcLogTime = jsonLog.Time.ToUniversalTime();
			jsonLog.Data[TIMESTAMP_FIELD] = utcLogTime.ToString(TIME_FORMAT, CultureInfo.InvariantCulture);

			var json = this.serializer.Serialize(jsonLog.Data);
			restRequest.AddRawJsonBody(json);

			var restResponse = this.restClient.ExecuteAsync(restRequest).GetAwaiter().GetResult();
			if(restResponse.IsSuccessful == true) { return; }

			throw this.FailedRequest("Unsuccessfull ElasticSearch Log", restRequest, json, restResponse);
		}

		public async Task Log(JsonLog[] jsonLogs, Action<JsonLog[]> onLogFailed) {
			var requestStringBuilder = new StringBuilder();

			var action = new IndexActionRequest();
			foreach(var jsonLog in jsonLogs) {
				action.Details.Id = jsonLog.Id;
				requestStringBuilder.AppendLine(this.serializer.Serialize(action));

				var utcLogTime = jsonLog.Time.ToUniversalTime();
				jsonLog.Data[TIMESTAMP_FIELD] = utcLogTime.ToString(TIME_FORMAT, CultureInfo.InvariantCulture);
				requestStringBuilder.AppendLine(this.serializer.Serialize(jsonLog.Data));
			}

			var restRequest = new RestRequest($"{this.Index}/_bulk", Method.Post);
			var json = requestStringBuilder.ToString();
			restRequest.AddStringBody(json, DataFormat.Json);

			var restResponse = await this.restClient.ExecuteAsync(restRequest);
			if(restResponse.IsSuccessful == false) {
				throw this.FailedRequest("Unsuccessfull ElasticSearch Log", restRequest, json, restResponse);
			}

			var response = JsonConvert.DeserializeObject<BulkResponse>(restResponse.Content!)
				?? throw this.FailedRequest($"Could not deserialize {nameof(BulkResponse)}", restRequest, json, restResponse);

			if(response.HasErrors is false) { return; }

			var failedLogs = response.Items
				.Where(i => i?.Index?.IsSuccessful is not true)
				.Select(failedLog => {
					var documentId = failedLog?.Index?.DocumentId;
					var jsonLog = jsonLogs.SingleOrDefault(jsonLog => jsonLog.Id == documentId);

					return jsonLog ??
						throw this.FailedRequest("A log failed and we could not trace which one", restRequest, json, restResponse);
				}).ToArray();

			onLogFailed(failedLogs);
			this.FailedRequest("Some logs failed", restRequest, json, restResponse);
		}

		private Exception FailedRequest(string message, RestRequest restRequest, string json, RestResponse restResponse) {
			Console.WriteLine(
				@$"{message}:
Request: POST {this.restClient.BuildUri(restRequest)} with ""{json}""
Response: Status {(int)restResponse.StatusCode} with Content ""{restResponse.Content}"" and Error ""{restResponse.ErrorMessage}""");
			return restResponse.ErrorException ?? new ApiResponseException(restResponse);
		}
	}
}
