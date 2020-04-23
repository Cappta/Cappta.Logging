using System;

namespace Cappta.Logging.Converters
{
	public class LogConverterFactory : ILogConverterFactory
	{
		private readonly int maxDepth;
		private readonly IObjectConverter[] objectSerializers;

		public LogConverterFactory(
			int maxDepth = 15,
			params IObjectConverter[] objectSerializers)
		{
			this.maxDepth = maxDepth;
			this.objectSerializers = objectSerializers ?? throw new ArgumentNullException(nameof(objectSerializers));
		}

		public ILogConverter Create()
			=> new LogConverter(this.maxDepth, this.objectSerializers);
	}
}
