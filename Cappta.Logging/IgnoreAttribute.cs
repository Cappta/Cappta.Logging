using System;

namespace Cappta.Logging {

	[AttributeUsage(AttributeTargets.Property)]
	public class IgnoreAttribute : Attribute {
	}
}