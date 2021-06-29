using RestSharp;

namespace Cappta.Logging.Extensions {
	internal static class IRestRequestExtensions {
		public static void AddRawJsonBody(this IRestRequest restRequest, string rawJson)
			=> restRequest.AddParameter("application/json", rawJson, ParameterType.RequestBody);
	}
}
