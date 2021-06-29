using System;

namespace Microsoft.Extensions.Logging {
	public static class ILoggerExtensions {
		public static void Log(this ILogger logger, LogLevel logLevel, Exception exception)
			=> logger.Log(logLevel, exception, string.Empty);

		public static void Log(this ILogger logger, LogLevel logLevel, EventId eventId, Exception exception)
			=> logger.Log(logLevel, eventId, exception, string.Empty);

		public static void LogCritical(this ILogger logger, Exception exception)
			=> logger.LogCritical(exception, string.Empty);

		public static void LogCritical(this ILogger logger, EventId eventId, Exception exception)
			=> logger.LogCritical(eventId, exception, string.Empty);

		public static void LogDebug(this ILogger logger, Exception exception)
			=> logger.LogDebug(exception, string.Empty);

		public static void LogDebug(this ILogger logger, EventId eventId, Exception exception)
			=> logger.LogDebug(eventId, exception, string.Empty);

		public static void LogError(this ILogger logger, Exception exception)
			=> logger.LogError(exception, string.Empty);

		public static void LogError(this ILogger logger, EventId eventId, Exception exception)
			=> logger.LogError(eventId, exception, string.Empty);

		public static void LogInformation(this ILogger logger, Exception exception)
			=> logger.LogInformation(exception, string.Empty);

		public static void LogInformation(this ILogger logger, EventId eventId, Exception exception)
			=> logger.LogInformation(eventId, exception, string.Empty);

		public static void LogTrace(this ILogger logger, Exception exception)
			=> logger.LogTrace(exception, string.Empty);

		public static void LogTrace(this ILogger logger, EventId eventId, Exception exception)
			=> logger.LogTrace(eventId, exception, string.Empty);

		public static void LogWarning(this ILogger logger, Exception exception)
			=> logger.LogWarning(exception, string.Empty);

		public static void LogWarning(this ILogger logger, EventId eventId, Exception exception)
			=> logger.LogWarning(eventId, exception, string.Empty);
	}
}