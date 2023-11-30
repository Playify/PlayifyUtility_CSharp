using JetBrains.Annotations;

namespace PlayifyUtility.Web.Utils;

[PublicAPI]
public class CloseException:Exception{
	public CloseException(){
	}

	public CloseException(string message):base(message){
	}

	public CloseException(string message,Exception innerException):base(message,innerException){
	}

	public CloseException(Exception innerException):base(null!,innerException){
	}
}