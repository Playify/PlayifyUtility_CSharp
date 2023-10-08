using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace PlayifyUtility.HelperClasses.Comparers;

[PublicAPI]
public class FileComparer:IComparer<string>{
	private static FileComparer? _instance;
	public static FileComparer Instance=>_instance??=new FileComparer();
	private FileComparer(){}
	private readonly Regex _splitter=new(@"(?<=\d)(?=\D)|(?<=\D)(?=\d)");

	public int Compare(string? x,string? y){
		var xx=_splitter.Split(x!);
		var yy=_splitter.Split(y!);

		foreach(var r in xx.Zip(yy,CompareFilePart))
			if(r!=0)
				return r;
		return xx.Length-yy.Length;
	}

	private static int CompareFilePart(string x,string y){
		if(x.Length!=0&&y.Length!=0&&//If non empty
		   x[0] is >='0' and <='9'&&y[0] is >='0' and <='9')//and both are numbers
			(x,y)=(x.PadLeft(y.Length,'0'),y.PadLeft(x.Length,'0'));//pad them both

		return StringComparer.OrdinalIgnoreCase.Compare(x,y);
	}
}