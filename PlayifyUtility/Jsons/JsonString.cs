using System.Text;
using System.Text.RegularExpressions;

namespace PlayifyUtility.Jsons;

public class JsonString:Json{
	private static readonly Regex ControlCharacters=new("[\0-\x1f]");
	private readonly string _string;

	public JsonString(string s)=>_string=s??throw new ArgumentNullException(nameof(s));

	public new static JsonString? Parse(string s){
		try{
			var reader=new StringReader(s);
			var parse=Parse(reader);
			return NextPeek(reader)!=-1?null:parse;
		} catch(Exception){
			return null;
		}
	}

	public new static JsonString? Parse(ref string s){
		try{
			var reader=new StringReader(s);
			var parse=Parse(reader);

			s=reader.ReadToEnd();

			return parse;
		} catch(Exception){
			return null;
		}
	}

	public new static JsonString Parse(TextReader r){
		if(NextRead(r)!='"') throw new JsonException();
		return ContinueParseString(r);
	}

	public override string ToString(string? indent){
		var str=new StringBuilder();
		Append(str,indent);
		return str.ToString();
	}

	public override void Append(StringBuilder str,string? indent)=>Escape(str,_string);
	public override Json DeepCopy()=>this;

	public override double AsNumber()=>double.TryParse(_string,out var v)?v:0;

	public override bool AsBoolean()=>_string.Equals("true",StringComparison.OrdinalIgnoreCase);

	public override string AsString()=>_string;

	public static string Escape(string? s){
		var str=new StringBuilder();
		Escape(str,s);
		return str.ToString();
	}

	public static string Unescape(string s){
		if(s.Length==0) throw new JsonException();
		var reader=new StringReader(s);
		if(reader.Read()!='"') throw new JsonException();
		return ContinueParseString(reader);
	}

	public static void Escape(StringBuilder str,string? s){
		if(s==null){
			str.Append("null");
			return;
		}
		str.Append('"');
		s=s.Replace("\\",@"\\");
		s=s.Replace("\"","\\\"");
		s=s.Replace("\b",@"\b");
		s=s.Replace("\f",@"\f");
		s=s.Replace("\r",@"\r");
		s=s.Replace("\n",@"\n");
		s=s.Replace("\t",@"\t");


		var match=ControlCharacters.Match(s);
		var from=0;
		while(match.Success){
			str.Append(s,from,match.Index-from);
			from=match.Index+match.Length;
			foreach(var c in match.Value) str.Append("\\u").Append($"{(int) c:X4}");
			match=match.NextMatch();
		}
		str.Append(s,from,s.Length-from);
		str.Append('"');
	}


	public override bool Equals(object? obj)=>obj is JsonString s&&s._string==_string;

	public override int GetHashCode()=>_string.GetHashCode();

	public static bool operator==(JsonString? a,JsonString? b)=>a?._string==b?._string;

	public static bool operator!=(JsonString? a,JsonString? b)=>!(a==b);


	public static implicit operator JsonString(string s)=>new(s);
}