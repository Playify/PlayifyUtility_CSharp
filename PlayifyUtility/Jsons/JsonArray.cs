using System.Collections;
using System.Text;

namespace PlayifyUtility.Jsons;

[Obsolete]
public class JsonArray:Json,IEnumerable<Json>{
	private readonly List<Json> _value=new();

	public JsonArray(){
	}

	public JsonArray(IEnumerable<Json> e){
		foreach(var json in e) Add(json);
	}

	public JsonArray(IEnumerable<double> e){
		foreach(var json in e) Add(json);
	}

	public JsonArray(IEnumerable<int> e){
		foreach(var json in e) Add(json);
	}

	public JsonArray(IEnumerable<long> e){
		foreach(var json in e) Add(json);
	}

	public JsonArray(IEnumerable<bool> e){
		foreach(var json in e) Add(json);
	}

	public JsonArray(IEnumerable<string> e){
		foreach(var json in e) Add(json);
	}

	public override Json this[int index]{
		get=>_value[index];
		set=>_value[index]=value;
	}

	public int Length=>_value.Count;

	public IEnumerator<Json> GetEnumerator()=>_value.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator()=>GetEnumerator();

	public new static JsonArray? Parse(string s)=>Json.Parse(s) as JsonArray;

	public new static JsonArray? Parse(ref string s){
		var old=s;
		if(Json.Parse(ref s) is JsonArray arr) return arr;
		s=old;
		return null;
	}

	public new static JsonArray Parse(TextReader r){
		if(NextPeek(r)!='[') throw new JsonException();
		return (JsonArray) Json.Parse(r);
	}

	public void Add(Json value)=>_value.Add(value);
	public void Add(int i,Json value)=>_value.Insert(i,value);
	public void Set(int i,Json value)=>_value[i]=value;

	public Json Get(int i)=>_value[i];

	public void Remove(int i)=>_value.RemoveAt(i);

	public override string ToString(string? indent){
		var str=new StringBuilder();
		Append(str,indent);
		return str.ToString();
	}

	public override void Append(StringBuilder str,string? indent){
		if(Length==0){
			str.Append("[]");
			return;
		}
		if(indent==null){
			str.Append('[');
			var first=true;
			foreach(var j in _value){
				if(first) first=false;
				else str.Append(',');
				str.Append(j.ToString(null));
			}
			str.Append(']');
		} else{
			str.Append("[\n");
			var start=str.Length-1;
			var first=true;
			foreach(var j in _value){
				if(first) first=false;
				else str.Append(",\n");
				str.Append(j.ToString(indent));
			}
			for(var i=str.Length-1;i>=start;i--)
				if(str[i]=='\n')
					str.Insert(i+1,indent);
			str.Append("\n]");
		}
	}

	public override Json DeepCopy()=>new JsonArray(this.Select(j=>j.DeepCopy()));

	public override double AsNumber()=>Length;

	public override string AsString()=>ToString();
	public override bool Equals(object? obj)=>obj is JsonArray other&&_value.SequenceEqual(other._value);

	public override int GetHashCode()=>_value.GetHashCode();

	public override bool AsBoolean()=>Length!=0;
}