using Newtonsoft.Json;

namespace Cappta.Logging.Models.ElasticSearch {
	internal class IndexActionDetails {
		public IndexActionDetails(string index) {
			this.Index = index;
		}

		[JsonProperty("_index")]
		public string Index { get; }


		[JsonProperty("_id")]
		public string? Id { get; set; }


		public static implicit operator IndexActionDetails(string index) {
			return new IndexActionDetails(index);
		}
	}
}
