using Newtonsoft.Json;

namespace Cappta.Logging.Models.ElasticSearch {
	public class BulkResponse {
		[JsonProperty("took")]
		public int? ProcessingMilisseconds { get; set; }

		[JsonProperty("errors")]
		public bool? HasErrors { get; set; }

		public BulkResponseItem[]? Items { get; set; }
	}
}
