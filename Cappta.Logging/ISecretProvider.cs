using System.Collections.Generic;

namespace Cappta.Logging {
	public interface ISecretProvider {
		string Protect(string value);

		void Protect(IDictionary<string, object?> dictionary);
	}
}
