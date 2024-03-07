namespace PlayifyUtility.Loggers;

public class NamedLogger:Logger{
	private readonly Logger _logger;
	private readonly string[] _names;

	public NamedLogger(Logger logger,params string[] names):base(logger){
		_logger=logger;
		_names=names;
	}

	public override IEnumerable<string> Tags(LogLevel level){
		foreach(var tag in _logger.Tags(level))
			yield return tag;
		foreach(var name in _names)
			yield return name;
	}
}