using Cappta.Logging.Models.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cappta.Logging.Converters {
	public class LogConverter : ILogConverter {
		private readonly List<IObjectConverter> objectSerializers = new();
		private readonly int maxDepth;
		private readonly ISecretProvider secretProvider;
		private readonly Stack<Type> typeStack = new();
		private readonly Stack<object> objectStack = new();

		public LogConverter(
			int maxDepth,
			IObjectConverter[] objectSerializers,
			ISecretProvider secretProvider
		) {
			this.maxDepth = maxDepth;
			this.secretProvider = secretProvider;
			this.objectSerializers.AddRange(objectSerializers);
			this.objectSerializers.Add(MainObjectConverter.Instance);
		}

		public object? ConvertToLogObject(object? obj) {
			if(obj is null || this.objectStack.Contains(obj)) { return null; }
			try {
				this.typeStack.Push(obj.GetType());
				this.objectStack.Push(obj);

				if(this.typeStack.Count > this.maxDepth) { throw new DepthStackOverflowException(this.maxDepth, this.typeStack.Reverse().ToArray()); }

				foreach(var objectSerializer in this.objectSerializers) {
					obj = objectSerializer.Convert(obj, this, this.secretProvider);
				}
				return obj;
			} finally { this.typeStack.Pop(); this.objectStack.Pop(); }
		}
	}
}
