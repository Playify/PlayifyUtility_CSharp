using JetBrains.Annotations;

namespace PlayifyUtility.HelperClasses;

[PublicAPI]
public sealed class TemporarySetValue<T>:IDisposable{
	private readonly ISet<T> _set;
	public readonly T Value;

	public TemporarySetValue(ISet<T> set,T value){
		_set=set;
		Value=value;
		_set.Add(value);
	}

	public void Dispose()=>_set.Remove(Value);
}