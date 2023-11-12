using System.Globalization;

namespace PlayifyUtility.Jsons;

[Obsolete]
public class JsonNumber:Json{
	private readonly double _number;
	public JsonNumber(double number)=>_number=number;

	public new static JsonNumber? Parse(string s)=>Json.Parse(s) as JsonNumber;

	public new static JsonNumber? Parse(ref string s){
		var old=s;
		if(Json.Parse(ref s) is JsonNumber arr) return arr;
		s=old;
		return null;
	}

	public new static JsonNumber Parse(TextReader r){
		return Json.Parse(r) is JsonNumber n?n:throw new JsonException();
	}

	public override string ToString(string? indent)=>_number.ToString(CultureInfo.InvariantCulture);

	public override Json DeepCopy()=>this;

	public override double AsNumber()=>_number;

	public override bool AsBoolean()=>_number!=0;

	public override string AsString()=>_number.ToString(CultureInfo.InvariantCulture);

	// ReSharper disable once CompareOfFloatsByEqualityOperator
	public override bool Equals(object? obj)=>obj is JsonNumber num&&num._number==_number;

	public override int GetHashCode()=>_number.GetHashCode();

	// ReSharper disable once CompareOfFloatsByEqualityOperator
	public static bool operator==(JsonNumber? a,JsonNumber? b)=>a?._number==b?._number;

	public static bool operator!=(JsonNumber? a,JsonNumber? b)=>!(a==b);

	public static implicit operator JsonNumber(double number)=>new(number);
	public static implicit operator JsonNumber(int number)=>new(number);
	public static implicit operator JsonNumber(long number)=>new(number);
}