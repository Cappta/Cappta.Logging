using System;
using System.Collections.Generic;

namespace Cappta.Logging.Models {
	public class JsonLog {
		public JsonLog(IDictionary<string, object?> data) : this(data, DateTimeOffset.Now) { }
		public JsonLog(IDictionary<string, object?> data, DateTimeOffset time) {
			this.Data = data;
			this.Time = time;
		}

		public DateTimeOffset Time { get; set; }
		public IDictionary<string, object?> Data { get; set; }
	}
}
