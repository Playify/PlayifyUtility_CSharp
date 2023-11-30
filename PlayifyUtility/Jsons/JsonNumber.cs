using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using JetBrains.Annotations;
using PlayifyUtility.Utils.Extensions;

namespace PlayifyUtility.Jsons;

[PublicAPI]
public class JsonNumber:Json{
	public readonly double Value;
	public JsonNumber(double value)=>Value=value;

	#region Parse
	public static bool TryParse(string s,[MaybeNullWhen(false)]out JsonNumber json)=>TryParseGeneric(s,out json,ParseOrNull);
	public static bool TryParse(ref string s,[MaybeNullWhen(false)]out JsonNumber json)=>TryParseGeneric(ref s,out json,ParseOrNull);
	public static bool TryParse(TextReader s,[MaybeNullWhen(false)]out JsonNumber json)=>ParseOrNull(s).NotNull(out json);


	public new static JsonNumber? ParseOrNull(string s)=>TryParse(s,out var json)?json:null;
	public new static JsonNumber? ParseOrNull(ref string s)=>TryParse(ref s,out var json)?json:null;
	public new static JsonNumber? ParseOrNull(TextReader r){
		var c=NextRead(r);
		if(c=='N') return r.Read()=='a'&&r.Read()=='N'?new JsonNumber(double.NaN):null;
		if(c=='I') return "nfinity".All(n=>r.Read()==n)?new JsonNumber(double.PositiveInfinity):null;

		if(c is not ((>='0' and <='9') or '+' or '-')) return null;
		var builder=new StringBuilder();
		builder.Append((char) c);
		c=r.Peek();
		if(c=='I')
			return builder[0] is '+' or '-'&&"Infinity".All(n=>r.Read()==n)
			       ?new JsonNumber(builder[0]=='+'
			                       ?double.PositiveInfinity
			                       :double.NegativeInfinity)
			       :null;

		var allowDot=true;
		var allowE=true;
		var allowSign=false;
		while(true){
			switch(c){
				case >='0' and <='9':
					builder.Append((char) c);
					break;
				case '.' when allowDot:
					builder.Append('.');
					allowDot=false;
					break;
				case 'e' or 'E' when allowE&&(builder.Length>1||builder[0]!='-'):
					builder.Append((char) c);
					allowE=false;
					allowSign=true;
					allowDot=false;

					r.Read();//remove peeked value from stream
					continue;
				case '+' or '-' when allowSign:
					builder.Append((char) c);
					break;
				default:{
					return double.TryParse(builder.ToString(),NumberStyles.Any,CultureInfo.InvariantCulture,out var v)?new JsonNumber(v):null;
				}
			}
			r.Read();//remove peeked value from stream
			allowSign=false;
			c=r.Peek();
		}
	}
	#endregion

	#region Convert
	public override string ToString(string? indent)=>AsString();

	public override Json DeepCopy()=>this;

	public override double AsDouble()=>Value;

	public override bool AsBool()=>Value!=0;

	public override string AsString()=>Value.ToString(CultureInfo.InvariantCulture);
	#endregion

	#region Operators
	// ReSharper disable once CompareOfFloatsByEqualityOperator
	public override bool Equals(object? obj)=>obj is JsonNumber other&&other.Value==Value;

	public override int GetHashCode()=>Value.GetHashCode();

	// ReSharper disable once CompareOfFloatsByEqualityOperator
	public static bool operator==(JsonNumber l,JsonNumber r)=>l.Value==r.Value;
	public static bool operator!=(JsonNumber l,JsonNumber r)=>!(l==r);
	public static implicit operator double(JsonNumber j)=>j.Value;
	public static explicit operator int(JsonNumber j)=>(int) j.Value;
	public static explicit operator long(JsonNumber j)=>(long) j.Value;
	public static implicit operator JsonNumber(double b)=>new(b);
	public static implicit operator JsonNumber(int b)=>new(b);
	public static implicit operator JsonNumber(long b)=>new(b);
	#endregion
}