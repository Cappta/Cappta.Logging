using Cappta.Logging.Models.Exceptions;
using System;
using System.Collections.Generic;

namespace Cappta.Logging.Converters
{
	public class LogConverter : ILogConverter
	{
		private readonly List<IObjectConverter> objectSerializers = new List<IObjectConverter>();
		private readonly int maxDepth;

		private readonly Stack<Type> typeStack = new Stack<Type>();

		public LogConverter(
			int maxDepth,
			params IObjectConverter[] objectSerializers)
		{
			this.maxDepth = maxDepth;

			this.objectSerializers.Add(MainObjectConverter.Instance);
			this.objectSerializers.AddRange(objectSerializers);
		}

		public object ConvertToLogObject(object obj)
		{
			try
			{
				this.typeStack.Push(obj?.GetType());

				if (this.typeStack.Count > this.maxDepth) { obj = new DepthStackOverflow(this.typeStack.ToArray()); }

				foreach (var objectSerializer in this.objectSerializers)
				{
					obj = objectSerializer.Convert(obj, this);
				}
				return obj;
			}
			finally { this.typeStack.Pop(); }
		}
	}
}
