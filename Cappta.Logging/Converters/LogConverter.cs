using System.Collections.Generic;

namespace Cappta.Logging.Converters
{
	public class LogConverter : ILogConverter
	{
		private readonly List<IObjectConverter> objectSerializers = new List<IObjectConverter>();

		public LogConverter(params IObjectConverter[] objectSerializers)
		{
			this.objectSerializers.Add(MainObjectConverter.Instance);
			this.objectSerializers.AddRange(objectSerializers);
		}

		public object ConvertToLogObject(object obj)
		{
			foreach (var objectSerializer in this.objectSerializers)
			{
				obj = objectSerializer.Convert(obj, this);
			}
			return obj;
		}
	}
}
