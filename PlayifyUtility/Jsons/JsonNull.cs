namespace PlayifyUtility.Jsons;

public class JsonNull:Json{
	public static readonly JsonNull Null=new();

	private JsonNull(){
	}

	public new static JsonNull? Parse(string s)=>Json.Parse(s) as JsonNull;

	public new static JsonNull? Parse(ref string s){
		var old=s;
		if(Json.Parse(ref s) is JsonNull arr) return arr;
		s=old;
		return null;
	}

	public new static JsonNull Parse(TextReader r){
		if(NextPeek(r)!='n') throw new JsonException();
		return (JsonNull) Json.Parse(r);
	}

	public override string ToString(string? indent)=>"null";

	public override Json DeepCopy()=>this;

	public override double AsNumber()=>0;

	public override bool AsBoolean()=>false;

	public override string? AsString()=>null;
	public override bool Equals(object? obj)=>obj is JsonNull;

	public override int GetHashCode()=>0;
}