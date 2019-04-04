using Cappta.Logging.Models;
using System.Collections.Generic;

namespace Cappta.Logging.Services
{
	public interface ILogService
	{
		void Log(JsonLog jsonLog);
		void Log(IDictionary<string, object> data);
	}
}
