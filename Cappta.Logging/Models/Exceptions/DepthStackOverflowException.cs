using Cappta.Logging.Converters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cappta.Logging.Models.Exceptions {
	public class DepthStackOverflowException : Exception, ILogConvertable {
		public DepthStackOverflowException(int maxDepth, Type[] typeStack)
			: base($"Breached the maximum depth of {maxDepth} when serializing log objects [{string.Join("; ", typeStack.Select((type, i) => $"#{i} - {type.FullName}"))}]") {
			this.MaxDepth = maxDepth;
			this.TypeStack = typeStack;
		}

		public int MaxDepth { get; }
		public Type[] TypeStack { get; }

		public object Convert(ILogConverter logSerializer, ISecretProvider secretProvider)
			=> new SortedDictionary<string, object?>(StringComparer.OrdinalIgnoreCase) {
				{ nameof(this.Message), this.Message },
				{ nameof(this.StackTrace), this.StackTrace },
				{ nameof(Type), this.GetType().FullName },
				{ nameof(this.InnerException), logSerializer.ConvertToLogObject(this.InnerException) },
				{ nameof(this.MaxDepth), this.MaxDepth },
				{ nameof(this.TypeStack), this.GetTypeStackLogObject() }
			};

		private object GetTypeStackLogObject()
			=> string.Join(
				Environment.NewLine,
				this.TypeStack.Reverse().Select((type, i) => $"#{i} - {type.FullName}")
			);
	}
}
