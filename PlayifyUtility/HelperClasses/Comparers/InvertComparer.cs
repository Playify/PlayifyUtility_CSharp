namespace PlayifyUtility.HelperClasses.Comparers;

public class InvertComparer<T>:IComparer<T>{
	private readonly IComparer<T> _other;

	private InvertComparer(IComparer<T> other){
		_other=other;
	}

	public int Compare(T? x,T? y)=>-_other.Compare(x,y);

	public static IComparer<T> Invert(IComparer<T> comparer)=>comparer is InvertComparer<T> already?already._other:new InvertComparer<T>(comparer);
}