using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using PlayifyUtility.Utils;
using PlayifyUtility.Utils.Extensions;

namespace PlayifyUtility.Jsons;

[PublicAPI]
public class JsonString:Json{
	public readonly string Value;
	public JsonString(string value)=>Value=value;

	#region Parse
	public static bool TryParse(string s,[MaybeNullWhen(false)]out JsonString json)=>TryParseGeneric(s,out json,ParseOrNull);
	public static bool TryParse(ref string s,[MaybeNullWhen(false)]out JsonString json)=>TryParseGeneric(ref s,out json,ParseOrNull);
	public static bool TryParse(TextReader s,[MaybeNullWhen(false)]out JsonString json)=>ParseOrNull(s).NotNull(out json);


	public new static JsonString? ParseOrNull(string s)=>TryParse(s,out var json)?json:null;
	public new static JsonString? ParseOrNull(ref string s)=>TryParse(ref s,out var json)?json:null;

	public new static JsonString? ParseOrNull(TextReader r){
		if(NextPeek(r)!='"') return null;
		return UnescapeOrNull(r) is{} s?new JsonString(s):null;
	}
	#endregion

	#region Convert
	public override Json DeepCopy()=>this;

	public override double AsDouble()=>double.TryParse(Value,out var v)?v:0;

	public override bool AsBool()=>Value.Equals("true",StringComparison.OrdinalIgnoreCase);

	public override string AsString()=>Value;
	#endregion

	#region ToString
	public override string ToString(string? indent)=>Escape(Value);

	public override StringBuilder Append(StringBuilder str,string? indent)=>Escape(str,Value);
	#endregion

	#region Operators
	public override bool Equals(object? obj)=>obj is JsonString other&&other.Value==Value;

	public override int GetHashCode()=>Value.GetHashCode();

	public static bool operator ==(JsonString l,JsonString r)=>l.Value==r.Value;
	public static bool operator !=(JsonString l,JsonString r)=>!(l==r);
	public static implicit operator string(JsonString j)=>j.Value;
	public static implicit operator JsonString(string b)=>new(b);
	#endregion

	#region Accessor
	private static readonly Regex ControlCharacters=new("[\0-\x1f]");
	public static string Escape(string? s)=>Escape(new StringBuilder(),s).ToString();

	public static StringBuilder Escape(StringBuilder str,string? s){
		if(s==null) return str.Append("null");
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
			foreach(var c in match.Value) str.Append("\\u").Append($"{(int)c:X4}");
			match=match.NextMatch();
		}
		str.Append(s,from,s.Length-from);
		str.Append('"');
		return str;
	}

	public static bool TryUnescape(string s,[MaybeNullWhen(false)]out string result){
		if(s.Length<2) return FunctionUtils.TryGetNever(out result);
		using var reader=new StringReader(s);
		return UnescapeOrNull(reader).NotNull(out result);
	}

	public static string? UnescapeOrNull(string s)=>TryUnescape(s,out var result)?result:null;

	public static string? UnescapeOrNull(TextReader r){
		if(r.Read()!='"') return null;

		var str=new StringBuilder();
		var escape=false;
		while(true)
			if(escape){
				switch(r.Read()){
					case -1:
						return null;/*
						case '\\':
							str.Append('\\');
							break;
						case '"':
							str.Append('"');
							break;
						case '/':
							str.Append('/');
							break;*/
					case 'b':
						str.Append('\b');
						break;
					case 'f':
						str.Append('\f');
						break;
					case 'r':
						str.Append('\r');
						break;
					case 'n':
						str.Append('\n');
						break;
					case 't':
						str.Append('\t');
						break;
					case 'u':
						var cp=0;
						for(var i=0;i<4;i++){
							cp<<=4;
							var c=r.Read();
							if(!(c switch{
									    >='0' and <='9'=>c-'0',
									    >='a' and <='f'=>(c-'a')+10,
									    >='A' and <='F'=>(c-'A')+10,
									    //-1=>throw new EndOfStreamException(),
									    _=>(int?)null,
								    }).TryGet(out var hex)) return null;
							cp|=hex;
						}
						str.Append(cp is >=55296 and <=57343
							           ?char.ToString((char)cp)//Surrogate codepoint value
							           :char.ConvertFromUtf32(cp));
						break;
					case var c://Defaults to just using the char as it is
						str.Append((char)c);
						break;
				}
				escape=false;
			} else
				switch(r.Read()){
					case '"':return str.ToString();
					//case -1:throw new EndOfStreamException();
					case -1:return null;
					case '\\':
						escape=true;
						break;
					case var c:
						str.Append((char)c);
						break;
				}
	}
	#endregion

}