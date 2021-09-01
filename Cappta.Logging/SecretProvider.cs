using Cappta.Logging.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Cappta.Logging {
	public class SecretProvider : ISecretProvider {
		private ConcurrentDictionary<string, string> SecretHashDict = new();

		public string Protect(string value) {
			return this.SecretHashDict.GetOrAdd(value, this.ComputeHash);
		}

		public string ComputeHash(string value) {
			using var sha256 = SHA256.Create();
			var secretBytes = Encoding.UTF8.GetBytes(value);
			var hashBytes = sha256.ComputeHash(secretBytes);
			var base64Sha256 = Convert.ToBase64String(hashBytes);
			return $"|SHA256:{base64Sha256}|";
		}

		public void Protect(IDictionary<string, object?> dictionary) {
			var secrets = this.SecretHashDict.Keys;
			if(secrets.Any() == false) { return; }

			foreach(var key in dictionary.Keys.ToArray()) {
				var value = dictionary[key];

				if(value is null) { continue; }
				if(value is IDictionary<string, object?> subDict) {
					this.Protect(subDict);
					continue;
				}
				if(value is not string) { continue; }

				var valueString = value.ToString();
				if(DateTimeOffset.TryParse(valueString, out _)) { continue; }

				var exposedSecrets = secrets.Where(secret => valueString.ContainsIgnoringCase(secret)).ToArray();
				if(exposedSecrets.Any() == false) { continue; }

				var protectedStringBuilder = new StringBuilder(valueString);
				foreach(var secret in exposedSecrets) {
					protectedStringBuilder.Replace(secret, this.SecretHashDict[secret]);
				}
				dictionary[key] = protectedStringBuilder.ToString();
			}
		}
	}
}
