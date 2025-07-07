using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace PlayifyUtility.Utils.Extensions;

[PublicAPI]
public static class VariableExtensions{
	public static T Push<T>(this T t,out T to)=>to=t;

	public static bool TryOverride<T>(this ref T t,T to) where T : struct{
		if(Equals(t,to)) return false;
		t=to;
		return true;
	}

	public static bool TryOverride<T>(this ref T? t,T? to) where T : struct{
		if(Equals(t,to)) return false;
		t=to;
		return true;
	}

	//[Obsolete("Instead use: input is{} output")] //when using out parameter directly, then this is not possible using pattern matching
	public static bool NotNull<T>(this T? t,[MaybeNullWhen(false)]out T result) where T : class?{
		result=t;
		return t!=null;
	}

	//[Obsolete("Instead use: input is{} output")] //when using out parameter directly, then this is not possible using pattern matching
	public static bool TryGet<T>(this T? t,out T result) where T : struct{
		result=t.GetValueOrDefault();
		return t.HasValue;
	}

	public static bool TryCast<T,TResult>(this T? t,out TResult result) where TResult : T{
		if(t is TResult res){
			result=res;
			return true;
		}
		result=default!;
		return false;
	}
}