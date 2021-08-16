using System;
using System.Collections.Generic;

namespace Cappta.Logging.Models {
	public class JsonLog {
		public JsonLog(IDictionary<string, object?> data) : this(data, DateTimeOffset.Now) { }
		public JsonLog(IDictionary<string, object?> data, DateTimeOffset time) {
			this.Data = data;
			this.Time = time;
			this.Id = Guid.NewGuid().ToString("N");
		}

		public DateTimeOffset Time { get; }
		public IDictionary<string, object?> Data { get; }
		public string Id { get; }
	}
}
