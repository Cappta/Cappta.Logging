using RestSharp;

namespace Cappta.Logging.Extensions {
	internal static class RestRequestExtensions {
		public static void AddRawJsonBody(this RestRequest restRequest, string rawJson)
			=> restRequest.AddStringBody(rawJson, DataFormat.Json);
	}
}
