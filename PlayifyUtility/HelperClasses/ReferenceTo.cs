using JetBrains.Annotations;

namespace PlayifyUtility.HelperClasses;

[PublicAPI]
public class ReferenceTo<T>{
	public T Value;

	public ReferenceTo()=>Value=default!;
	public ReferenceTo(T value)=>Value=value;

	public static implicit operator T(ReferenceTo<T> @ref)=>@ref.Value;
	public static explicit operator ReferenceTo<T>(T t)=>new(t);
}