using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace PlayifyUtility.Utils;

[PublicAPI]
public static class StringUtils{
	public static readonly Regex CommentRegex=new(@"\/\/.*|\/\*(?s:.*?)\*\/");

	public static IEnumerable<string> Split(this string str,Predicate<char> controller){
		var nextPiece=0;

		for(var c=0;c<str.Length;c++){
			if(!controller(str[c])) continue;
			yield return str.Substring(nextPiece,c-nextPiece);
			nextPiece=c+1;
		}
		yield return str[nextPiece..];
	}
}