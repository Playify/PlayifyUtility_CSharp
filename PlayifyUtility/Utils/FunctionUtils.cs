using System.Diagnostics.CodeAnalysis;

namespace PlayifyUtility.Utils;

public static class FunctionUtils{
	public static bool TryGetNever<T>([MaybeNullWhen(false)]out T t){
		t=default;
		return false;
	}

	[DoesNotReturn]
	public static void NeverReturns(){
		while(true) Thread.Sleep(Timeout.Infinite);
		// ReSharper disable once FunctionNeverReturns
	}
}