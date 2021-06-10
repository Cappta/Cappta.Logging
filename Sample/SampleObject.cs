using Cappta.Logging;

namespace Sample
{
	public class SampleObject
	{
		public string Data { get; set; }

		[Secret]
		public string Secret { get; set; }
	}
}
