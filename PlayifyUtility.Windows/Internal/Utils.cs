using System.Text;

namespace PlayifyUtility.Windows.Internal;

internal static class Utils{
	public static bool TryPop<T>(this Stack<T> t,/*[MaybeNullWhen(false)]*/out T pop){
		if(t.Count==0){
			pop=default!;
			return false;
		}
		pop=t.Pop();
		return true;
	}

	public static string ReplaceLineEndings(this string thiz)=>thiz.ReplaceLineEndings(Environment.NewLine);

	public static string ReplaceLineEndings(this string thiz,string replacementText){
		if(replacementText is null)
			throw new ArgumentNullException(nameof(replacementText));

		var idxOfFirstNewlineChar=IndexOfNewlineChar(thiz,out var stride);
		if(idxOfFirstNewlineChar<0)
			return thiz;

		var firstSegment=thiz.Substring(0,idxOfFirstNewlineChar);
		var remaining=thiz.Substring(idxOfFirstNewlineChar+stride);

		var builder=new StringBuilder();
		builder.Append(firstSegment);
		while(true){
			var idx=IndexOfNewlineChar(remaining,out stride);
			if(idx<0)
				break;

			builder.Append(replacementText);
			builder.Append(remaining.Substring(0,idx));
			remaining=remaining.Substring(idx+stride);
		}

		builder.Append(replacementText);
		builder.Append(remaining);
		return builder.ToString();
	}

	private static int IndexOfNewlineChar(string text,out int stride){
		const string needles="\r\n\f\u0085\u2028\u2029";
		stride=default;
		var idx=text.IndexOfAny(needles.ToCharArray());
		if(idx<0||idx>=text.Length)
			return idx;

		stride=1;// needle found
		if(text[idx]!='\r')
			return idx;

		var nextCharIdx=idx+1;
		if(nextCharIdx<text.Length&&text[nextCharIdx]=='\n'){
			stride=2;
		}

		return idx;
	}
}