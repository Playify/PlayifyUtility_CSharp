using JetBrains.Annotations;

namespace PlayifyUtility.HelperClasses.Comparers;

[PublicAPI]
[Obsolete("Use ComparerUtils.Func instead")]
public class ComparerFromFunc<T>(Func<T,T,int> compare):IComparer<T>{
	public int Compare(T? x,T? y)=>compare(x!,y!);
}