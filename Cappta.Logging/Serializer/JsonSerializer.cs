using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Globalization;

namespace Cappta.Logging.Serializer {
	public class JsonSerializer : ISerializer {
		public JsonSerializer()
			=> this.JsonSerializerSettings = new JsonSerializerSettings() {
				Converters = new List<JsonConverter>() { new StringEnumConverter() },
				Culture = CultureInfo.InvariantCulture,
				NullValueHandling = NullValueHandling.Ignore,
			};

		public JsonSerializerSettings JsonSerializerSettings { get; set; }

		public virtual string Serialize(object obj)
			=> JsonConvert.SerializeObject(obj, this.JsonSerializerSettings);
	}
}
