using Cappta.Logging.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;
using Npgsql;
using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Cappta.Logging.Converters {
	internal class MainObjectConverter : IObjectConverter {
		private const string OriginalFormatKey = "{OriginalFormat}";

		private MainObjectConverter() { }

		public static MainObjectConverter Instance { get; } = new MainObjectConverter();

		public object? Convert(
			object? obj,
			ILogConverter logSerializer,
			ISecretProvider secretProvider
		) {
			if(obj == null) { return null; }
			switch(obj) {
				case DateTime dateTime:
					return dateTime.ToString();
				case DateTimeOffset dateTimeOffset:
					return dateTimeOffset.ToString();
				case Enum enumValue:
					return enumValue;
				case Guid guid:
					return guid.ToString();
				case JToken jToken:
					return this.ConvertJToken(jToken, logSerializer);
				case IHeaderDictionary headerDictionary:
					return this.ConvertHeaderDictionary(headerDictionary, secretProvider);
				case IDictionary<string, object> stringObjectDictionary:
					return this.ConvertDictionary(stringObjectDictionary, logSerializer);
				case IDictionary<string, StringValues> stringStringValuesDictionary:
					return this.ConvertStringStringValuesDictionary(stringStringValuesDictionary);
				case IEnumerable<KeyValuePair<string, object>> kvpEnumerable:
					return this.ConvertKvpEnumerable(kvpEnumerable, logSerializer);
				case ILogConvertable logConvertable:
					return logConvertable.Convert(logSerializer, secretProvider);
				case MethodInfo methodInfo:
					return methodInfo.ToString();
				case string stringValue:
					return stringValue;
				case decimal decimalValue:
					return decimalValue;
				case Thread thread:
					return this.ConvertThread(thread);
				case TimeSpan timeSpan:
					return timeSpan.ToString("g", CultureInfo.InvariantCulture);
				case Type type:
					return type.ToString();
				case Uri uri:
					return uri.ToString();
				case CancellationToken cancellationToken:
					return this.ConvertCancellationToken(cancellationToken);
				case IEnumerable<NpgsqlParameter> npgsqlParameterEnumerable:
					return this.ConvertNpgsqlParameterEnumerable(npgsqlParameterEnumerable, logSerializer);
				case NpgsqlParameter npgsqlParameter:
					return this.ConvertNpgsqlParameter(npgsqlParameter, logSerializer);

				case TaskCanceledException taskCanceledException:
					return this.ConvertTaskCanceledException(taskCanceledException, logSerializer);
				case AggregateException aggregateException:
					return this.ConvertAggregateException(aggregateException, logSerializer);
				case Exception ex:
					return this.ConvertException(ex);
				case IEnumerable enumerable:
					return this.ConvertEnumerable(enumerable, logSerializer).ToArray();
				case RestClient restClient:
					return this.ConvertRestClient(restClient);
				case RestRequest restRequest:
					return this.ConvertRestRequest(restRequest, logSerializer);
				case RestResponse restResponse:
					return this.ConvertRestResponse(restResponse, logSerializer);
				default:
					if(obj.GetType().IsPrimitive) { return obj; }
					return this.Reflect(obj, logSerializer, secretProvider);
			}
		}

		private object? ConvertJToken(JToken token, ILogConverter logSerializer) {
			switch(token.Type) {
				case JTokenType.Object:
					var jObject = token as JObject;
					var dict = new SortedDictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
					foreach(var jProperty in jObject!.Properties()) {
						dict.Add(jProperty.Name, logSerializer.ConvertToLogObject(jProperty.Value));
					}
					return dict;

				case JTokenType.Array:
					var jArray = token as JArray;
					var array = jArray!.ToArray();
					return logSerializer.ConvertToLogObject(array);

				case JTokenType.Integer:
					return (long)token;

				case JTokenType.Float:
					return (decimal)token;

				case JTokenType.Null:
					return null;

				case JTokenType.Boolean:
					return (bool)token;

				default:
					return token.ToString();
			}
		}

		private object ConvertTaskCanceledException(TaskCanceledException taskCanceledException, ILogConverter logSerializer) {
			var dict = new SortedDictionary<string, object?>(StringComparer.OrdinalIgnoreCase) {
				{ "StackTrace", taskCanceledException.StackTrace },
				{ "Type", taskCanceledException.GetType().FullName },
				{ "InnerException", logSerializer.ConvertToLogObject(taskCanceledException.InnerException) },
			};
			return dict;
		}

		private object ConvertAggregateException(AggregateException aggregateException, ILogConverter logSerializer) {
			var dict = new SortedDictionary<string, object?>(StringComparer.OrdinalIgnoreCase) {
				{ "StackTrace", aggregateException.StackTrace },
				{ "Type", aggregateException.GetType().FullName },
				{ "InnerException", logSerializer.ConvertToLogObject(aggregateException.InnerException) },
			};
			for(var i = 1; i < aggregateException.InnerExceptions.Count; i++) {
				dict.Add($"InnerException{i + 1}", logSerializer.ConvertToLogObject(aggregateException.InnerExceptions[i]));
			}
			return dict;
		}

		private static readonly string[] SENSITIVE_HEADERS = new[] { "authorization" };
		private object ConvertHeaderDictionary(IHeaderDictionary headerDictionary, ISecretProvider secretProvider) {
			var dict = new SortedDictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
			foreach(var kvp in headerDictionary) {
				var key = kvp.Key;
				var isSensitive = SENSITIVE_HEADERS.Contains(key.ToLower());
				foreach(var value in kvp.Value) {
					if(value is null) { continue; }
					dict.ForceAdd(key, isSensitive ? secretProvider.Protect(value) : value);
				}
			}
			return dict;
		}

		private object ConvertDictionary(IDictionary<string, object> dictionary, ILogConverter logSerializer)
			=> dictionary.ToDictionary(kvp => kvp.Key, kvp => logSerializer.ConvertToLogObject(kvp.Value));

		private object ConvertStringStringValuesDictionary(IDictionary<string, StringValues> dictionary) {
			var dict = new SortedDictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
			foreach(var kvp in dictionary) {
				var key = kvp.Key;
				foreach(var value in kvp.Value) {
					dict.ForceAdd(key, value);
				}
			}
			return dict;
		}

		private object ConvertException(Exception ex) {
			var dict = new SortedDictionary<string, object?>(StringComparer.OrdinalIgnoreCase) {
				{ "Message", ex.Message },
				{ "StackTrace", ex.StackTrace },
				{ "Type", ex.GetType().FullName },
				{ "Json", ex.ToJson() },
			};
			return dict;
		}

		private IEnumerable<object?> ConvertEnumerable(IEnumerable enumerable, ILogConverter logSerializer) {
			var enumerator = enumerable.GetEnumerator();

			while(enumerator.MoveNext()) {
				yield return logSerializer.ConvertToLogObject(enumerator.Current);
			}
		}

		private object ConvertRestClient(RestClient restClient) {
			var optionsPropertyInfo = typeof(RestClient).GetProperty("Options", BindingFlags.Instance | BindingFlags.NonPublic);

			var options = optionsPropertyInfo?.GetValue(restClient) as RestClientOptions;

			return new SortedDictionary<string, object?>(StringComparer.OrdinalIgnoreCase) {
				{ "BaseUri", options?.BaseUrl?.ToString() ?? options?.BaseHost}
			};
		}

		private object ConvertRestRequest(RestRequest restRequest, ILogConverter logSerializer) {
			var requestBody = restRequest.Parameters.SingleOrDefault(p => p.Type == ParameterType.RequestBody);

			return new SortedDictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
			{
				{ "Resource", restRequest.Resource },
				{ "Method", restRequest.Method },
				{ "Body", logSerializer.ConvertToLogObject(requestBody?.Value) }
			};
		}

		private object ConvertRestResponse(RestResponse restResponse, ILogConverter logSerializer)
			=> new SortedDictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
			{
				{ "Content", restResponse.Content },
				{ "Exception", logSerializer.ConvertToLogObject(restResponse.ErrorException) },
				{ "Uri", logSerializer.ConvertToLogObject(restResponse.ResponseUri) },
				{ "State", restResponse.ErrorMessage },
				{ "Status", restResponse.StatusCode },
				{ "StatusCode", (int)restResponse.StatusCode },
			};

		private object ConvertKvpEnumerable(IEnumerable<KeyValuePair<string, object>> kvpEnumerable, ILogConverter logSerializer) {
			var kvpListDict = new SortedDictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
			foreach(var kvp in kvpEnumerable) {
				if(kvp.Key.Contains(".")) {
					var hierarchy = kvp.Key.Split('.');

					var dict = kvpListDict.CreateOrGetSubDict(hierarchy[0]);
					for(var i = 1; i < hierarchy.Length - 1; i++) {
						dict = dict.CreateOrGetSubDict(hierarchy[i]);
					}

					dict.Add(hierarchy.Last(), logSerializer.ConvertToLogObject(kvp.Value));
					continue;
				}

				if(kvp.Key == OriginalFormatKey) {
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

		private object ConvertNpgsqlParameterEnumerable(IEnumerable<NpgsqlParameter> npgsqlParameterEnumerable, ILogConverter logSerializer) {
			var dict = new SortedDictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
			foreach(var npgsqlParameter in npgsqlParameterEnumerable) {
				dict.ForceAdd(npgsqlParameter.ParameterName, logSerializer.ConvertToLogObject(npgsqlParameter.Value));
			}
			return dict;
		}

		private object ConvertNpgsqlParameter(NpgsqlParameter npgsqlParameter, ILogConverter logSerializer)
			=> new SortedDictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
			{
				{ npgsqlParameter.ParameterName, logSerializer.ConvertToLogObject(npgsqlParameter.Value) },
			};

		private object Reflect(object obj, ILogConverter logSerializer, ISecretProvider secretProvider) {
			var dict = new SortedDictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
			var objType = obj.GetType();
			foreach(var prop in objType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)) {
				if(prop.GetMethod is null) { continue; }
				if(prop.GetCustomAttribute<IgnoreAttribute>() is not null) { continue; }

				var value = GetPropertyValue(obj, prop, secretProvider);

				dict.ForceAdd(prop.Name, logSerializer.ConvertToLogObject(value));
			}
			return dict;
		}

		private static object? GetPropertyValue(object obj, PropertyInfo prop, ISecretProvider secretProvider) {
			try {
				var value = prop.GetValue(obj);

				if(value is null || prop.GetCustomAttribute<SecretAttribute>() is null) { return value; }

				var stringValue = value.ToString();
				return secretProvider.Protect(stringValue);
			} catch(Exception ex) {
				return ex;
			}
		}
	}
}
