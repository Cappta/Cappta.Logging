using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Globalization;

namespace Cappta.Logging.Extensions {
	public static class ObjectExtensions {
		private static readonly JsonSerializerSettings JSON_SETTINGS = new() {
			Converters = new List<JsonConverter>() { new StringEnumConverter() },
			Culture = CultureInfo.InvariantCulture,
			NullValueHandling = NullValueHandling.Ignore,
			ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
		};

		public static string ToJson(this object value) {
			return value.ToJson(JSON_SETTINGS);
		}

		public static string ToJson(this object value, JsonSerializerSettings jsonSerializerSettings) {
			return JsonConvert.SerializeObject(value, jsonSerializerSettings);
		}
	}
}
