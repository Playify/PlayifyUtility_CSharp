using System.Text;
using System.Text.RegularExpressions;

namespace PlayifyUtility.Utils;

public partial class PlatformUtils{

	public static List<string> SplitArguments(string commandLine,bool removeHashComments=true){
		var builder=new StringBuilder(commandLine.Length);
		var list=new List<string>();
		var i=0;

		while(i<commandLine.Length){
			while(i<commandLine.Length&&char.IsWhiteSpace(commandLine[i])) i++;
			if(i==commandLine.Length) break;
			if(commandLine[i]=='#'&&removeHashComments) break;

			var quoteCount=0;
			builder.Length=0;
			while(i<commandLine.Length&&(!char.IsWhiteSpace(commandLine[i])||(quoteCount%2!=0))){
				var current=commandLine[i];
				switch(current){
					case '\\':{
						var slashCount=0;
						do{
							builder.Append(commandLine[i]);
							i++;
							slashCount++;
						} while(i<commandLine.Length&&commandLine[i]=='\\');

						// Slashes not followed by a quote character can be ignored for now
						if(i>=commandLine.Length||commandLine[i]!='"') break;

						// If there is an odd number of slashes then it is escaping the quote
						// otherwise it is just a quote.
						if(slashCount%2==0) quoteCount++;

						builder.Append('"');
						i++;
						break;
					}
					case '"':
						builder.Append(current);
						quoteCount++;
						i++;
						break;
					default:
						if((current<0x1||current>0x1f)&&current!='|') builder.Append(current);
						i++;
						break;
				}
			}

			// If the quote string is surrounded by quotes with no interior quotes then 
			// remove the quotes here. 
			if(quoteCount==2&&builder[0]=='"'&&builder[^1]=='"'){
				builder.Remove(0,1);
				builder.Remove(builder.Length-1,1);
			}

			if(builder.Length>0) list.Add(builder.ToString());
		}

		return list;
	}


	public static string EscapeArguments(params string[] args){
		var arguments=new StringBuilder();
		var invalidChar=new Regex("[\x00\x0a\x0d]");//these can not be escaped
		var needsQuotes=new Regex(@"\s|""");//contains whitespace or two quote characters
		var escapeQuote=new Regex(@"(\\*)(""|$)");//one or more '\' followed with a quote or end of string
		for(var arg=0;arg<args.Length;arg++){
			if(args[arg]==null) throw new ArgumentNullException("args["+arg+"]");
			if(invalidChar.IsMatch(args[arg])) throw new ArgumentOutOfRangeException("args["+arg+"]");
			if(args[arg]==string.Empty) arguments.Append("\"\"");
			else if(!needsQuotes.IsMatch(args[arg])) arguments.Append(args[arg]);
			else{
				arguments.Append('"');
				arguments.Append(escapeQuote.Replace(args[arg],m=>
				                                     m.Groups[1].Value+m.Groups[1].Value+
				                                     (m.Groups[2].Value=="\""?"\\\"":"")));
				arguments.Append('"');
			}
			if(arg+1<args.Length) arguments.Append(' ');
		}
		return arguments.ToString();
	}

	public static string MakeSafeFileName(string s)=>Path.GetInvalidFileNameChars().Aggregate(s,(curr,c)=>curr.Replace(c,'#'));
}