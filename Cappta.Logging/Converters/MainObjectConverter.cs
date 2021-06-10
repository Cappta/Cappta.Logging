using Cappta.Logging.Extensions;
using Microsoft.Extensions.Primitives;
using Npgsql;
using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Cappta.Logging.Converters
{
	internal class MainObjectConverter : IObjectConverter
	{
		private const string OriginalFormatKey = "{OriginalFormat}";

		private MainObjectConverter() { }

		public static MainObjectConverter Instance { get; } = new MainObjectConverter();

		public object? Convert(object? obj, ILogConverter logSerializer)
		{
			if (obj == null) { return null; }
			switch (obj)
			{
				case DateTime dateTime: return dateTime.ToString();
				case DateTimeOffset dateTimeOffset: return dateTimeOffset.ToString();
				case Enum enumValue: return enumValue;
				case Guid guid: return guid.ToString();
				case IDictionary<string, object> stringObjectDictionary: return this.ConvertDictionary(stringObjectDictionary, logSerializer);
				case IDictionary<string, StringValues> stringStringValuesDictionary: return this.ConvertStringStringValuesDictionary(stringStringValuesDictionary);
				case IEnumerable<KeyValuePair<string, object>> kvpEnumerable: return this.ConvertKvpEnumerable(kvpEnumerable, logSerializer);
				case ILogConvertable logConvertable: return logConvertable.Convert(logSerializer);
				case MethodInfo methodInfo: return methodInfo.ToString();
				case string stringValue: return stringValue;
				case Thread thread: return this.ConvertThread(thread);
				case TimeSpan timeSpan: return timeSpan.ToString("g", CultureInfo.InvariantCulture);
				case Type type: return type.ToString();
				case Uri uri: return uri.ToString();
				case CancellationToken cancellationToken: return this.ConvertCancellationToken(cancellationToken);
				case IEnumerable<NpgsqlParameter> npgsqlParameterEnumerable: return this.ConvertNpgsqlParameterEnumerable(npgsqlParameterEnumerable, logSerializer);
				case NpgsqlParameter npgsqlParameter: return this.ConvertNpgsqlParameter(npgsqlParameter, logSerializer);

				case AggregateException aggregateException: return this.ConvertAggregateException(aggregateException, logSerializer);
				case Exception ex: return this.ConvertException(ex, logSerializer);
				case IEnumerable enumerable: return this.ConvertEnumerable(enumerable, logSerializer);
				case IRestClient restClient: return this.ConvertIRestClient(restClient, logSerializer);
				case IRestRequest restRequest: return this.ConvertIRestRequest(restRequest, logSerializer);
				case IRestResponse restResponse: return this.ConvertIRestResponse(restResponse, logSerializer);
				default:
					if (obj.GetType().IsPrimitive) { return obj; }
					return this.Reflect(obj, logSerializer);
			}
		}

		private object ConvertAggregateException(AggregateException aggregateException, ILogConverter logSerializer)
		{
			var dict = new SortedDictionary<string, object?>(StringComparer.OrdinalIgnoreCase) {
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

		private object ConvertStringStringValuesDictionary(IDictionary<string, StringValues> dictionary)
		{
			var dict = new SortedDictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
			foreach(var kvp in dictionary)
			{
				var key = kvp.Key;
				foreach(var value in kvp.Value)
				{
					dict.ForceAdd(key, value);
				}
			}
			return dict;
		}

		private object ConvertEnumerable(IEnumerable enumerable, ILogConverter logSerializer)
		{
			var objects = enumerable.Cast<object>();
			var logObjects = objects.Select((obj, index) => (obj: logSerializer.ConvertToLogObject(obj), index));
			return logObjects.ToDictionary(tuple => tuple.index.ToString(), tuple => tuple.obj);
		}

		private object ConvertException(Exception ex, ILogConverter logSerializer)
		{
			var dict = new SortedDictionary<string, object?>(StringComparer.OrdinalIgnoreCase) {
				{ "Message", ex.Message },
				{ "StackTrace", ex.StackTrace },
				{ "Type", ex.GetType().FullName },
				{ "InnerException", logSerializer.ConvertToLogObject(ex.InnerException) },
			};
			this.AppendExtendedExceptionProperties(dict, ex.GetType(), ex, logSerializer);
			return dict;
		}

		private void AppendExtendedExceptionProperties(IDictionary<string, object?> dict, Type type, Exception ex, ILogConverter logSerializer)
		{
			if (type == typeof(Exception)) { return; }

			foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.DeclaredOnly))
			{
				dict.ForceAdd(prop.Name, logSerializer.ConvertToLogObject(prop.GetValue(ex)));
			}

			this.AppendExtendedExceptionProperties(dict, type.BaseType, ex, logSerializer);
		}

		private object ConvertIRestClient(IRestClient restClient, ILogConverter logSerializer)
			=> new SortedDictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
			{
				{ "BaseUri", restClient.BaseUrl}
			};

		private object ConvertIRestRequest(IRestRequest restRequest, ILogConverter logSerializer)
		{
			var requestBody = restRequest.Parameters.SingleOrDefault(p => p.Type == ParameterType.RequestBody);

			return new SortedDictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
			{
				{ "Resource", restRequest.Resource },
				{ "Method", restRequest.Method },
				{ "Body", logSerializer.ConvertToLogObject(requestBody?.Value) }
			};
		}

		private object ConvertIRestResponse(IRestResponse restResponse, ILogConverter logSerializer)
			=> new SortedDictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
			{
				{ "Content", restResponse.Content },
				{ "Exception", logSerializer.ConvertToLogObject(restResponse.ErrorException) },
				{ "Header", logSerializer.ConvertToLogObject(restResponse.Headers.ToDictionary(p => p.Name, p => p.Value)) },
				{ "State", restResponse.ErrorMessage },
				{ "Status", restResponse.StatusCode },
				{ "StatusCode", (int)restResponse.StatusCode },
			};

		private object ConvertKvpEnumerable(IEnumerable<KeyValuePair<string, object>> kvpEnumerable, ILogConverter logSerializer)
		{
			var kvpListDict = new SortedDictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
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
			=> new SortedDictionary<string, object>(StringComparer.OrdinalIgnoreCase)
			{
				{ "IsBackground", thread.IsBackground },
				{ "IsThreadPoolThread", thread.IsThreadPoolThread },
				{ "Priority", thread.Priority },
				{ "State", thread.ThreadState },
				{ "Name", thread.Name },
			};

		private object ConvertCancellationToken(CancellationToken cancellationToken)
			=> new SortedDictionary<string, object>(StringComparer.OrdinalIgnoreCase)
			{
				{ "IsCancellationRequested", cancellationToken.IsCancellationRequested },
			};

		private object ConvertNpgsqlParameterEnumerable(IEnumerable<NpgsqlParameter> npgsqlParameterEnumerable, ILogConverter logSerializer)
		{
			var dict = new SortedDictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
			foreach (var npgsqlParameter in npgsqlParameterEnumerable)
			{
				dict.ForceAdd(npgsqlParameter.ParameterName, logSerializer.ConvertToLogObject(npgsqlParameter.Value));
			}
			return dict;
		}

		private object ConvertNpgsqlParameter(NpgsqlParameter npgsqlParameter, ILogConverter logSerializer)
			=> new SortedDictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
			{
				{ npgsqlParameter.ParameterName, logSerializer.ConvertToLogObject(npgsqlParameter.Value) },
			};

		private object Reflect(object obj, ILogConverter logSerializer)
		{
			var dict = new SortedDictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
			var objType = obj.GetType();
			foreach (var prop in objType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty))
			{
				var value = GetPropertyValue(obj, prop);

				dict.ForceAdd(prop.Name, logSerializer.ConvertToLogObject(value));
			}
			return dict;
		}

		private static object? GetPropertyValue(object obj, PropertyInfo prop)
		{
			try
			{
				var value =  prop.GetValue(obj);

				if (value is null || prop.GetCustomAttribute<SecretAttribute>() is null) { return value; }

				var stringValue = value.ToString();

				using var sha256 = SHA256.Create();
				var secretBytes = Encoding.UTF8.GetBytes(stringValue);
				var hashBytes = sha256.ComputeHash(secretBytes);
				var base64Sha256 = System.Convert.ToBase64String(hashBytes);
				return $"SHA256:{base64Sha256}";
			}
			catch (Exception ex)
			{
				return ex;
			}
		}
	}
}
