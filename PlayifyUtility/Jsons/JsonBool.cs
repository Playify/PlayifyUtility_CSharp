using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using PlayifyUtility.Utils.Extensions;

namespace PlayifyUtility.Jsons;

[PublicAPI]
public class JsonBool:Json{
	public static readonly JsonBool True=new(true);
	public static readonly JsonBool False=new(false);
	public static JsonBool Get(bool b)=>b?True:False;
	
	public readonly bool Value;

	
	private JsonBool(bool value)=>Value=value;

	#region Parse
	public static bool TryParse(string s,[MaybeNullWhen(false)]out JsonBool json)=>TryParseGeneric(s,out json,ParseOrNull);
	public static bool TryParse(ref string s,[MaybeNullWhen(false)]out JsonBool json)=>TryParseGeneric(ref s,out json,ParseOrNull);
	public static bool TryParse(TextReader s,[MaybeNullWhen(false)]out JsonBool json)=>ParseOrNull(s).NotNull(out json);


	public new static JsonBool? ParseOrNull(string s)=>TryParse(s,out var json)?json:null;
	public new static JsonBool? ParseOrNull(ref string s)=>TryParse(ref s,out var json)?json:null;
	public new static JsonBool? ParseOrNull(TextReader r)
		=>NextRead(r) switch{
			't' when r.Read()=='r'&&r.Read()=='u'&&r.Read()=='e'=>True,
			'f' when r.Read()=='a'&&r.Read()=='l'&&r.Read()=='s'&&r.Read()=='e'=>False,
			_=>null,
		};
	#endregion

	#region Convert

	public override Json DeepCopy()=>this;

	public override double AsDouble()=>Value?1:0;

	public override bool AsBool()=>Value;

	public override string AsString()=>Value?"true":"false";
	
	public override string ToString(string? indent)=>AsString();
	#endregion

	#region Operators
	public override bool Equals(object? obj)=>ReferenceEquals(this,obj);

	public override int GetHashCode()=>Value?1:0;

	public static bool operator==(JsonBool l,JsonBool r)=>ReferenceEquals(l,r);
	public static bool operator!=(JsonBool l,JsonBool r)=>!(l==r);
	public static implicit operator bool(JsonBool j)=>j.Value;
	public static implicit operator JsonBool(bool b)=>b?True:False;
	#endregion
}