using Newtonsoft.Json;
using System;

namespace Cappta.Logging.Models.ElasticSearch {
	public class ErrorResponse {
		public string? Type { get; set; }

		public string? Reason { get; set; }

		[JsonProperty("caused_by")]
		public ErrorResponse? CausedBy { get; set; }

		public override string ToString() {
			var causedBy = this.CausedBy is null ? string.Empty :
				$"{Environment.NewLine} caused by {this.CausedBy}";
			return $"{this.Type}:\"{this.Reason}\"{causedBy}";
		}
	}
}
