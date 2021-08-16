using Newtonsoft.Json;

namespace Cappta.Logging.Models.ElasticSearch {
	internal class IndexActionRequest {
		public IndexActionRequest(IndexActionDetails details) {
			this.Details = details;
		}

		[JsonProperty("index")]
		public IndexActionDetails Details { get; }
	}
}
