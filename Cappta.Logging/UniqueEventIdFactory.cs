using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;

namespace Cappta.Logging {
	public class UniqueEventIdFactory {
		private readonly ConcurrentDictionary<int, string?> idNameDict = new();

		public static UniqueEventIdFactory Instance { get; } = new();

		public EventId Create(int id, string? name = null) {
			if(this.idNameDict.TryAdd(id, name)) {
				return new EventId(id, name);
			}

			var conflictingName = this.idNameDict[id];
			throw new InvalidOperationException($"Event of Id \"{id}\" and Name \"{name}\" is conflicting with the already registered Event Named \"{conflictingName}\"");
		}
	}
}
