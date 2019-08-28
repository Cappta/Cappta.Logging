using Cappta.Logging.Converters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cappta.Logging.Models.Exceptions
{
	public class DepthStackOverflow : ILogConvertable
	{
		public DepthStackOverflow(Type[] types)
			=> this.Types = types;

		public Type[] Types { get; }

		public object Convert(ILogConverter logSerializer)
			=> new SortedDictionary<string, object>()
			{
				{ nameof(DepthStackOverflow), this.GetTypesLogObject() },
			};

		private object GetTypesLogObject()
			=> this.Types.Reverse()
				.Select((type, i) => (type, i))
				.ToDictionary(_ => _.i, _ => _.type?.ToString());
	}
}
