namespace PlayifyUtility.Jsons;

[Obsolete]
public class JsonBool:Json{
	public static readonly JsonBool True=new(true);
	public static readonly JsonBool False=new(false);
	private readonly bool _b;

	private JsonBool(bool b)=>_b=b;

	public new static JsonBool? Parse(string s)=>Json.Parse(s) as JsonBool;

	public new static JsonBool? Parse(ref string s){
		var old=s;
		if(Json.Parse(ref s) is JsonBool arr) return arr;
		s=old;
		return null;
	}

	public new static JsonBool Parse(TextReader r){
		if(NextPeek(r) is not ('t' or 'f')) throw new JsonException();
		return (JsonBool) Json.Parse(r);
	}

	public override string ToString(string? indent)=>_b?"true":"false";

	public override Json DeepCopy()=>this;

	public override double AsNumber()=>_b?1:0;

	public override bool AsBoolean()=>_b;

	public override string AsString()=>_b?"true":"false";
	public override bool Equals(object? obj)=>obj is JsonBool b&&_b==b._b;

	public override int GetHashCode()=>_b?1:0;

	public static implicit operator JsonBool(bool b)=>b?True:False;
}