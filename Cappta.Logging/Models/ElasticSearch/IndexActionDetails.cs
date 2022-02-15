using Newtonsoft.Json;

namespace Cappta.Logging.Models.ElasticSearch {
	internal class IndexActionDetails {
		[JsonProperty("_index")]
		public string? Index { get; set; }


		[JsonProperty("_id")]
		public string? Id { get; set; }


		public static implicit operator IndexActionDetails(string index) {
			return new IndexActionDetails() { Index = index };
		}
	}
}
