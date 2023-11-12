using System.Diagnostics.CodeAnalysis;
using System.Runtime.ExceptionServices;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace PlayifyUtility.Utils;

[PublicAPI]
public static class ExceptionUtils{/*
	private static readonly Action<Exception> PreserveInternalException=((Action<Exception>)
		                                                                    Delegate
		                                                                    .CreateDelegate(typeof(Action<Exception>),
		                                                                                    typeof(Exception)
		                                                                                    .GetMethod("InternalPreserveStackTrace",
		                                                                                               BindingFlags.Instance|BindingFlags.NonPublic)!));*/


	[DoesNotReturn]
	public static T Rethrow<T>(this T exception) where T:Exception{
		ExceptionDispatchInfo.Capture(exception).Throw();
		return exception;
	}

	public static T PreserveStackTrace<T>(this T e) where T:Exception{
		/*
		PreserveInternalException(e);
		/*/
		var ctx=new StreamingContext(StreamingContextStates.CrossAppDomain);
		var mgr=new ObjectManager(null,ctx);
		var si=new SerializationInfo(e.GetType(),new FormatterConverter());

		e.GetObjectData(si,ctx);
		mgr.RegisterObject(e,1,si);
		mgr.DoFixups();
		//*/
		return e;
	}

	public static void Test(){
		var e=new Exception();
		throw e.Rethrow();

	}
}