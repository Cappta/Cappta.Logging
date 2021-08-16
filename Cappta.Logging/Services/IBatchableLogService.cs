using Cappta.Logging.Models;
using System;

namespace Cappta.Logging.Services {
	public interface IBatchableLogService {
		void Log(JsonLog[] jsonLogs, Action<JsonLog[]> onLogFailed);
	}
}
