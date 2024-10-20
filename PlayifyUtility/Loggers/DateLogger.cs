namespace PlayifyUtility.Loggers;

public class DateLogger:Logger{
	private readonly Logger _logger;
	private readonly string _format;
	private readonly IFormatProvider? _provider;


	public DateLogger(Logger logger,string format,IFormatProvider? provider=null):base(logger){
		_logger=logger;
		_format=format;
		_provider=provider;
	}

	public override IEnumerable<string> Tags(LogLevel level){
		foreach(var tag in _logger.Tags(level))
			yield return tag;
		yield return DateTime.Now.ToString(_format,_provider);
	}
}