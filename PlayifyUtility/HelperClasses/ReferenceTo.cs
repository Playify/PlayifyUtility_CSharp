using JetBrains.Annotations;

namespace PlayifyUtility.HelperClasses;

[PublicAPI]
public class ReferenceTo<T>(T value){
	public T Value=value;

	public ReferenceTo():this(default!){
	}

	public static implicit operator T(ReferenceTo<T> @ref)=>@ref.Value;
	public static explicit operator ReferenceTo<T>(T t)=>new(t);
}