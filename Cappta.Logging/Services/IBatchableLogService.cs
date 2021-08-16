using Cappta.Logging.Models;
using System;
using System.Threading.Tasks;

namespace Cappta.Logging.Services {
	public interface IBatchableLogService {
		Task Log(JsonLog[] jsonLogs, Action<JsonLog[]> onLogFailed);
	}
}
