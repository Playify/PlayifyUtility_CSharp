namespace PlayifyUtility.Loggers;

public class AnsiLessLogger:Logger{
	private bool _isActive;

	public AnsiLessLogger(TextWriter writer):base(writer){}


	public override void Write(char value){
		if(value=='\u001b') _isActive=true;//Start if escape sequence
		else if(_isActive) _isActive=!char.IsLetter(value);//Deactivate if letter
		else base.Write(value);
	}

	public override void Write(char[] buffer,int index,int count){
		for(var i=0;i<count;i++) Write(buffer[index+i]);
	}

	public override void Write(string? value){
		if(value==null) return;
		foreach(var c in value) Write(c);
	}

	public override Task WriteAsync(char value)=>Task.Run(()=>Write(value));
	public override Task WriteAsync(char[] buffer,int index,int count)=>Task.Run(()=>Write(buffer,index,count));
	public override Task WriteAsync(string? value)=>Task.Run(()=>Write(value));
}