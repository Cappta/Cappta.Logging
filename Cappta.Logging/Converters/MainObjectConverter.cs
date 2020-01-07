using Cappta.Logging.Extensions;
using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
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
				case IDictionary<string, object> stringObjectDictionary: return this.ConvertDictionary(stringObjectDictionary, logSerializer);
				case IEnumerable<KeyValuePair<string, object>> kvpEnumerable: return this.ConvertKvpEnumerable(kvpEnumerable, logSerializer);
				case ILogConvertable logConvertable: return logConvertable.Convert(logSerializer);
				case MethodInfo methodInfo: return methodInfo.ToString();
				case string stringValue: return stringValue;
				case Thread thread: return this.ConvertThread(thread);
				case TimeSpan timeSpan: return timeSpan.ToString("g", CultureInfo.InvariantCulture);
				case Type type: return type.ToString();
				case Uri uri: return uri.ToString();

				case AggregateException aggregateException: return this.ConvertAggregateException(aggregateException, logSerializer);
				case Exception ex: return this.ConvertException(ex, logSerializer);
				case IEnumerable enumerable: return this.ConvertEnumerable(enumerable, logSerializer);
				case IRestRequest restRequest: return this.ConvertIRestRequest(restRequest, logSerializer);
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
				{ "Type", ex.GetType().FullName },
				{ "InnerException", logSerializer.ConvertToLogObject(ex.InnerException) },
			};
			this.AppendExtendedExceptionProperties(dict, ex.GetType(), ex, logSerializer);
			return dict;
		}

		private void AppendExtendedExceptionProperties(IDictionary<string, object> dict, Type type, Exception ex, ILogConverter logSerializer)
		{
			if(type == typeof(Exception)) { return; }

			foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.DeclaredOnly))
			{
				dict.ForceAdd(prop.Name, logSerializer.ConvertToLogObject(prop.GetValue(ex)));
			}

			this.AppendExtendedExceptionProperties(dict, type.BaseType, ex, logSerializer);
		}

		private object ConvertIRestRequest(IRestRequest restRequest, ILogConverter logSerializer)
		{
			var requestBody = restRequest.Parameters.SingleOrDefault(p => p.Type == ParameterType.RequestBody);

			return new SortedDictionary<string, object>()
			{
				{ "Resource", restRequest.Resource },
				{ "Method", restRequest.Method },
				{ "Body", logSerializer.ConvertToLogObject(requestBody?.Value) }
			};
		}

		private object ConvertIRestResponse(IRestResponse restResponse, ILogConverter logSerializer)
			=> new SortedDictionary<string, object>()
			{
				{ "Status", restResponse.StatusCode },
				{ "StatusCode", (int)restResponse.StatusCode },
				{ "Content", restResponse.Content },
				{ "State", restResponse.ErrorMessage },
				{ "Exception", logSerializer.ConvertToLogObject(restResponse.ErrorException) }
			};

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

				if (kvp.Key == OriginalFormatKey)
				{
					kvpListDict.ForceAdd("Message", kvpEnumerable.ToString());
					continue;
				}

				kvpListDict.ForceAdd(kvp.Key, logSerializer.ConvertToLogObject(kvp.Value));
			}
			return kvpListDict;
		}

		private object ConvertThread(Thread thread)
			=> new SortedDictionary<string, object>()
			{
				{ "IsBackground", thread.IsBackground },
				{ "IsThreadPoolThread", thread.IsThreadPoolThread },
				{ "Priority", thread.Priority },
				{ "State", thread.ThreadState },
				{ "Name", thread.Name },
			};

		private object Reflect(object obj, ILogConverter logSerializer)
		{
			var dict = new SortedDictionary<string, object>();
			var objType = obj.GetType();
			foreach (var prop in objType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty))
			{
				dict.ForceAdd(prop.Name, logSerializer.ConvertToLogObject(prop.GetValue(obj)));
			}
			return dict;
		}
	}
}
