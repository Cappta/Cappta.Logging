using Cappta.Logging.Converters;

namespace Cappta.Logging
{
	public interface ILogConvertable
	{
		object Convert(ILogConverter logSerializer);
	}
}
