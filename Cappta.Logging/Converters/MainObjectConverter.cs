using Cappta.Logging.Extensions;
using Microsoft.Extensions.Logging.Internal;
using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace Cappta.Logging.Converters
{
	internal class MainObjectConverter : IObjectConverter
	{
		private const string OriginalFormatKey = "{OriginalFormat}";

		private MainObjectConverter() { }

		public static MainObjectConverter Instance { get; } = new MainObjectConverter();

		public object Convert(object obj, ILogConverter logSerializer)
		{
			if (obj == null) { return null; }
			switch (obj)
			{
				case DateTime dateTime: return dateTime.ToString();
				case DateTimeOffset dateTimeOffset: return dateTimeOffset.ToString();
				case Enum enumValue: return enumValue;
				case Guid guid: return guid.ToString();
				case FormattedLogValues formattedLogValues: return this.ConvertFormattedLogValues(formattedLogValues, logSerializer);
				case IDictionary<string, object> stringObjectDictionary: return this.ConvertDictionary(stringObjectDictionary, logSerializer);
				case ILogConvertable logConvertable: return logConvertable.Convert(logSerializer);
				case IEnumerable<KeyValuePair<string, object>> kvpEnumerable: return this.ConvertKvpEnumerable(kvpEnumerable, logSerializer);
				case string stringValue: return stringValue;
				case Thread thread: return this.ConvertThread(thread);
				case TimeSpan timeSpan: return timeSpan.ToString("g", CultureInfo.InvariantCulture);

				case AggregateException aggregateException: return this.ConvertAggregateException(aggregateException, logSerializer);
				case Exception ex: return this.ConvertException(ex, logSerializer);
				case IEnumerable enumerable: return this.ConvertEnumerable(enumerable, logSerializer);
				case IRestResponse restResponse: return this.ConvertIRestResponse(restResponse, logSerializer);
				default:
					if (obj.GetType().IsPrimitive) { return obj; }
					return this.Reflect(obj, logSerializer);
			}
		}

		private object ConvertAggregateException(AggregateException aggregateException, ILogConverter logSerializer)
		{
			var dict = new SortedDictionary<string, object>() {
				{ "StackTrace", aggregateException.StackTrace },
				{ "Type", aggregateException.GetType().FullName },
				{ "InnerException", logSerializer.ConvertToLogObject(aggregateException.InnerException) },
			};
			for (var i = 1; i < aggregateException.InnerExceptions.Count; i++)
			{
				dict.Add($"InnerException{i + 1}", logSerializer.ConvertToLogObject(aggregateException.InnerExceptions[i]));
			}
			return dict;
		}
    
		private object ConvertDictionary(IDictionary<string, object> dictionary, ILogConverter logSerializer)
			=> dictionary.ToDictionary(kvp => kvp.Key, kvp => logSerializer.ConvertToLogObject(kvp.Value));

		private object ConvertEnumerable(IEnumerable enumerable, ILogConverter logSerializer)
		{
			var objects = enumerable.Cast<object>();
			var logObjects = objects.Select(o => logSerializer.ConvertToLogObject(o));
			return logObjects.ToArray();
		}

		private object ConvertException(Exception ex, ILogConverter logSerializer)
		{
			var dict = new SortedDictionary<string, object>() {
				{ "Message", ex.Message },
				{ "StackTrace", ex.StackTrace },
				{ "Type", ex.GetType().FullName }
			};
			if (ex.InnerException != null) { dict.Add("InnerException", logSerializer.ConvertToLogObject(ex.InnerException)); }
			return dict;
		}

		private object ConvertFormattedLogValues(FormattedLogValues formattedLogValues, ILogConverter logSerializer)
		{
			var dict = new SortedDictionary<string, object>() {
				{ "Message", formattedLogValues.ToString() }
			};

			foreach (var kvp in formattedLogValues)
			{
				if (kvp.Key == OriginalFormatKey) { continue; }

				dict.Add(kvp.Key, kvp.Value);
			}

			return dict;
		}

		private object ConvertIRestResponse(IRestResponse restResponse, ILogConverter logSerializer)
		{
			var dict = new SortedDictionary<string, object>()
			{
				{ "Status", restResponse.StatusCode },
				{ "StatusCode", (int)restResponse.StatusCode },
				{ "Content", restResponse.Content },
				{ "State", restResponse.ErrorMessage },
				{ "Exception", logSerializer.ConvertToLogObject(restResponse.ErrorException) }
			};
			return dict;
		}
		private object ConvertKvpEnumerable(IEnumerable<KeyValuePair<string, object>> kvpEnumerable, ILogConverter logSerializer)
		{
			var kvpListDict = new SortedDictionary<string, object>();
			foreach (var kvp in kvpEnumerable)
			{
				if (kvp.Key.Contains("."))
				{
					var hierarchy = kvp.Key.Split('.');

					var dict = kvpListDict.CreateOrGetSubDict(hierarchy[0]);
					for (var i = 1; i < hierarchy.Length - 1; i++)
					{
						dict = dict.CreateOrGetSubDict(hierarchy[i]);
					}

					dict.Add(hierarchy.Last(), logSerializer.ConvertToLogObject(kvp.Value));
					continue;
				}

				kvpListDict.ForceAdd(kvp.Key, logSerializer.ConvertToLogObject(kvp.Value));
			}
			return kvpListDict;
		}

		private object ConvertThread(Thread thread)
		{
			var dict = new SortedDictionary<string, object>()
			{
				{ "IsBackground", thread.IsBackground },
				{ "IsThreadPoolThread", thread.IsThreadPoolThread },
				{ "Priority", thread.Priority },
				{ "State", thread.ThreadState },
			};
			if (string.IsNullOrWhiteSpace(thread.Name) == false) { dict.Add("Name", thread.Name); }
			return dict;
		}
		private object Reflect(object obj, ILogConverter logSerializer)
		{
			var dict = new SortedDictionary<string, object>();
			var objType = obj.GetType();
			foreach (var prop in objType.GetProperties())
			{
				dict.ForceAdd(prop.Name, logSerializer.ConvertToLogObject(prop.GetValue(obj)));
			}
			return dict;
		}
	}
}
