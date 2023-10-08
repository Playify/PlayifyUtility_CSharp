namespace PlayifyUtility.HelperClasses.Comparers;

public class ComparerFromFunc<T>:IComparer<T>{
	private readonly Func<T,T,int> _compare;
	public ComparerFromFunc(Func<T,T,int> compare)=>_compare=compare;
	public int Compare(T? x,T? y)=>_compare(x!,y!);
}