using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace PlayifyUtility.Utils;

[PublicAPI]
public static class FunctionUtils{
	
	public static bool TryGetNever<T>([MaybeNullWhen(false)]out T t){
		t=default;
		return false;
	}
	public static T RunThenReturn<T>(Action a,T returnValue){
		a();
		return returnValue;
	}

	[DoesNotReturn]
	public static void SleepForever(){
		while(true) Thread.Sleep(Timeout.Infinite);
		// ReSharper disable once FunctionNeverReturns
	}

	public static Task DelayForever()=>Task.Delay(Timeout.Infinite);
}

public delegate bool TryParseFunction<in TSource,TResult>(TSource source,out TResult result);