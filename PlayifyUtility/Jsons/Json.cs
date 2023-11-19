using System.Diagnostics.CodeAnalysis;
using System.Text;
using JetBrains.Annotations;
using PlayifyUtility.Utils;

namespace PlayifyUtility.Jsons;

[PublicAPI]
public abstract partial class Json{
	#region Parse
	public static bool TryParse(string s,[MaybeNullWhen(false)]out Json json)=>TryParseGeneric(s,out json,ParseOrNull);
	public static bool TryParse(ref string s,[MaybeNullWhen(false)]out Json json)=>TryParseGeneric(ref s,out json,ParseOrNull);
	public static bool TryParse(TextReader s,[MaybeNullWhen(false)]out Json json)=>ParseOrNull(s).NotNull(out json);


	public static Json? ParseOrNull(string s)=>TryParse(s,out var json)?json:null;
	public static Json? ParseOrNull(ref string s)=>TryParse(ref s,out var json)?json:null;

	public static Json? ParseOrNull(TextReader r)
		=>NextPeek(r) switch{
			'{'=>JsonObject.ParseOrNull(r),
			'['=>JsonArray.ParseOrNull(r),
			'"'=>JsonString.ParseOrNull(r),
			'n'=>JsonNull.ParseOrNull(r),
			't' or 'f'=>JsonBool.ParseOrNull(r),
			_=>JsonNumber.ParseOrNull(r),
		};
	#endregion

	#region Convert
	public abstract Json DeepCopy();
	public virtual double AsDouble()=>throw new NotSupportedException("Can't convert "+GetType().Name+" to double");
	public virtual bool AsBool()=>throw new NotSupportedException("Can't convert "+GetType().Name+" to bool");
	public virtual string AsString()=>throw new NotSupportedException("Can't convert "+GetType().Name+" to string");
	public virtual JsonArray AsArray()=>throw new NotSupportedException("Can't convert "+GetType().Name+" to array");
	public virtual JsonObject AsObject()=>throw new NotSupportedException("Can't convert "+GetType().Name+" to object");
	#endregion

	#region ToString
	public override string ToString()=>ToString(null);
	public string ToPrettyString()=>ToString("\t");
	public string ToString(int indentSpaces)=>ToString(indentSpaces<0?new string('\t',-indentSpaces):new string(' ',indentSpaces));
	public abstract string ToString(string? indent);

	public virtual StringBuilder Append(StringBuilder str,string? indent)=>str.Append(ToString(indent));
	#endregion

	#region Operators
	public abstract override bool Equals(object? obj);
	public abstract override int GetHashCode();

	public static explicit operator bool(Json j)=>j.AsBool();
	public static explicit operator double(Json j)=>j.AsDouble();
	public static explicit operator int(Json j)=>(int) j.AsDouble();
	public static explicit operator long(Json j)=>(long) j.AsDouble();
	public static explicit operator string(Json j)=>j.AsString();

	public static implicit operator Json(bool b)=>b?JsonBool.True:JsonBool.False;
	public static implicit operator Json(double number)=>new JsonNumber(number);
	public static implicit operator Json(int number)=>new JsonNumber(number);
	public static implicit operator Json(long number)=>new JsonNumber(number);
	public static implicit operator Json(string? s)=>s!=null?new JsonString(s):JsonNull.Null;

	public static implicit operator Json(bool? nullable)=>nullable.TryGet(out var nonnull)?nonnull:JsonNull.Null;
	public static implicit operator Json(double? nullable)=>nullable.TryGet(out var nonnull)?nonnull:JsonNull.Null;
	public static implicit operator Json(int? nullable)=>nullable.TryGet(out var nonnull)?nonnull:JsonNull.Null;
	public static implicit operator Json(long? nullable)=>nullable.TryGet(out var nonnull)?nonnull:JsonNull.Null;
	#endregion

	#region Accessor
	public virtual int Count=>0;
	public virtual Json this[string property]{
		get=>throw new NotSupportedException(GetType().Name+" has no property access");
		set=>throw new NotSupportedException(GetType().Name+" has no property access");
	}
	public virtual Json this[int index]{
		get=>throw new NotSupportedException(GetType().Name+" has no index access");
		set=>throw new NotSupportedException(GetType().Name+" has no index access");
	}
	public virtual bool Has(string property)=>false;
	
	
	public virtual bool TryGet(string property,[MaybeNullWhen(false)]out Json json)=>FunctionUtils.TryGetNever(out json);
	public virtual bool TryGet(int index,[MaybeNullWhen(false)]out Json json)=>FunctionUtils.TryGetNever(out json);
	
	
	public bool TryGetArray(string property,[MaybeNullWhen(false)]out JsonArray json)=>GetArray(property).NotNull(out json);
	public bool TryGetArray(int index,[MaybeNullWhen(false)]out JsonArray json)=>GetArray(index).NotNull(out json);
	
	public bool TryGetObject(string property,[MaybeNullWhen(false)]out JsonObject json)=>GetObject(property).NotNull(out json);
	public bool TryGetObject(int index,[MaybeNullWhen(false)]out JsonObject json)=>GetObject(index).NotNull(out json);
	
	
	public Json? Get(string property)=>TryGet(property,out var json)?json:null;
	public Json? Get(int index)=>TryGet(index,out var json)?json:null;
	
	
	public JsonArray? GetArray()=>this as JsonArray;
	public JsonArray? GetArray(string property)=>Get(property) as JsonArray;
	public JsonArray? GetArray(int index)=>Get(index) as JsonArray;
	
	
	public JsonObject? GetObject()=>this as JsonObject;
	public JsonObject? GetObject(string property)=>Get(property) as JsonObject;
	public JsonObject? GetObject(int index)=>Get(index) as JsonObject;
	#endregion
}