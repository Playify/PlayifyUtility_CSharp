using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using PlayifyUtility.HelperClasses;

namespace PlayifyUtility.Utils.Extensions;

[PublicAPI]
public static class VariableExtensions{
	public static T Push<T>(this T t,out T to)=>to=t;

	public static bool TryOverride<T>(this ref T t,T to) where T:struct{
		if(Equals(t,to)) return false;
		t=to;
		return true;
	}

	public static bool NotNull<T>(this T? t,[MaybeNullWhen(false)]out T result) where T:class{
		result=t;
		return t!=null;
	}

	public static bool TryGet<T>(this T? t,out T result) where T:struct{
		result=t.GetValueOrDefault();
		return t.HasValue;
	}
	
	//Helper function if the user wants to write 'new StructType(args).Value=true;'
	public static ref T AsInlineEditable<T>(this T t) where T:struct=>ref new ReferenceTo<T>(t).Value;
}