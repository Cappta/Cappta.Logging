using Newtonsoft.Json;
using System;

namespace Cappta.Logging.Models
{
	internal class SplunkHecRequest
	{
		private static readonly DateTimeOffset UNIX_EPOCH = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

		public SplunkHecRequest() { }
		public SplunkHecRequest(string host, JsonLog entry)
		{
			if (string.IsNullOrWhiteSpace(host)) { throw new ArgumentNullException(nameof(host)); }

			this.Time = (entry.Time - UNIX_EPOCH).TotalSeconds;
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
