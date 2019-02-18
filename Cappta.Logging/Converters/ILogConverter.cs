namespace Cappta.Logging.Converters
{
	public interface ILogConverter
	{
		object ConvertToLogObject(object obj);
	}
}
