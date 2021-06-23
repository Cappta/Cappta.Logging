using Cappta.Logging;

namespace Sample {
	public class SampleObject {
		public string DataWithoutGet { set => this.Data = value; } //Without a getter, it's not logged

		public string Data { get; set; } //Public with getter are logged normally

		[Ignore]
		public string IgnoredData { get; set; } //Ignored attributes are not logged

		[Secret]
		public string Secret { get; set; } //Secret attribute hashes the value, so it's safe but can be checked

		private string PrivateSecret => this.Secret; //Private properties are not logged

		public string Field; //Fields are not logged, even if public
	}
}