using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.Linq;

namespace Cappta.Logging.Extensions
{
	public static class ModelStateDictionaryExtensions
	{
		public static IDictionary<string, object> ToRawValueDictionary(this ModelStateDictionary modelStateDictionary)
			=> modelStateDictionary.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.RawValue);
	}
}
