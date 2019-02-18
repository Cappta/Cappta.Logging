using Newtonsoft.Json;
using System;

namespace Cappta.Logging.Models
{
	internal class SplunkHecRequest
	{
		public SplunkHecRequest() { }
		public SplunkHecRequest(string host, JsonLog entry)
		{
			if (string.IsNullOrWhiteSpace(host)) { throw new ArgumentNullException(nameof(host)); }

			this.Time = (entry.Time - DateTimeOffset.UnixEpoch).TotalSeconds;
			this.Host = host;
			this.Event = entry.Data;
		}

		[JsonProperty("host")]
		public string Host { get; set; }

		[JsonProperty("time")]
		public double? Time { get; set; }

		[JsonProperty("event")]
		public object Event { get; set; }
	}
}
