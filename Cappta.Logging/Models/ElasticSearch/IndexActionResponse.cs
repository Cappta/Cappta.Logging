using Newtonsoft.Json;

namespace Cappta.Logging.Models.ElasticSearch {
	public class IndexActionResponse {
		[JsonProperty("_id")]
		public string? DocumentId { get; set; }

		[JsonProperty("status")]
		public int? StatusCode { get; set; }

		public ErrorResponse? Error { get; set; }

		public bool IsSuccessful => this.StatusCode is not null and < 300;
	}
}
