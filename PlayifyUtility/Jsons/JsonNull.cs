using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using PlayifyUtility.Utils;

namespace PlayifyUtility.Jsons;

[PublicAPI]
public class JsonNull:Json{
	public static readonly JsonNull Null=new();

	private JsonNull(){}

	#region Parse
	public static bool TryParse(string s,[MaybeNullWhen(false)]out JsonNull json)=>TryParseGeneric(s,out json,ParseOrNull);
	public static bool TryParse(ref string s,[MaybeNullWhen(false)]out JsonNull json)=>TryParseGeneric(ref s,out json,ParseOrNull);
	public static bool TryParse(TextReader s,[MaybeNullWhen(false)]out JsonNull json)=>ParseOrNull(s).NotNull(out json);


	public new static JsonNull? ParseOrNull(string s)=>TryParse(s,out var json)?json:null;
	public new static JsonNull? ParseOrNull(ref string s)=>TryParse(ref s,out var json)?json:null;
	public new static JsonNull? ParseOrNull(TextReader r)=>NextRead(r)=='n'&&r.Read()=='u'&&r.Read()=='l'&&r.Read()=='l'?Null:null;
	#endregion

	#region Convert

	public override Json DeepCopy()=>this;
	public override string ToString(string? indent)=>"null";
	#endregion

	#region Operators
	public override bool Equals(object? obj)=>obj is JsonNull;

	public override int GetHashCode()=>-1;
	#endregion
}