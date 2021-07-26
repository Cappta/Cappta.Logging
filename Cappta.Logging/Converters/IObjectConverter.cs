namespace Cappta.Logging.Converters {
	public interface IObjectConverter {
		object? Convert(
			object? obj,
			ILogConverter logSerializer,
			ISecretProvider secretProvider
		);
	}
}
