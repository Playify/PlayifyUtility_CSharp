using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace PlayifyUtility.Jsons;

public class JsonObject:Json,IEnumerable<KeyValuePair<string,Json>>{
	private readonly Dictionary<string,Json> _dictionary=new();
	private readonly List<string> _order=new();

	public int Length=>_order.Count;

	public override Json this[string index]{
		get=>_dictionary[index];
		set=>Put(index,value);
	}

	public IEnumerator<KeyValuePair<string,Json>> GetEnumerator()=>_dictionary.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator()=>GetEnumerator();

	public new static JsonObject? Parse(string s)=>Json.Parse(s) as JsonObject;

	public new static JsonObject? Parse(ref string s){
		var old=s;
		if(Json.Parse(ref s) is JsonObject arr) return arr;
		s=old;
		return null;
	}

	public new static JsonObject Parse(TextReader r){
		if(NextPeek(r)!='{') throw new JsonException();
		return (JsonObject) Json.Parse(r);
	}

	public void Add(string key,Json? json){
		_dictionary.Add(key,json??JsonNull.Null);
		_order.Add(key);
	}

	public void Put(string key,Json? value){
		_order.Remove(key);
		_order.Add(key);
		_dictionary[key]=value??JsonNull.Null;
	}

	public bool Has(string key)=>_dictionary.ContainsKey(key);
	public Json? Get(string key)=>_dictionary.TryGetValue(key,out var value)?value:null;
	public bool TryGet(string key,[MaybeNullWhen(false)] out Json json)=>_dictionary.TryGetValue(key,out json);
	
	public void Remove(string key){
		_order.Remove(key);
		_dictionary.Remove(key);
	}


	public override string ToString(string? indent){
		var str=new StringBuilder();
		Append(str,indent);
		return str.ToString();
	}

	public override void Append(StringBuilder str,string? indent){
		if(Length==0){
			str.Append("{}");
			return;
		}
		if(indent==null){
			str.Append('{');
			var first=true;
			foreach(var s in _order){
				if(first) first=false;
				else str.Append(',');
				JsonString.Escape(str,s);
				str.Append(':');
				str.Append(_dictionary[s].ToString(null));
			}
			str.Append('}');
		} else{
			str.Append("{\n");
			var start=str.Length-1;
			var first=true;
			foreach(var s in _order){
				if(first) first=false;
				else str.Append(",\n");
				JsonString.Escape(str,s);
				str.Append(':');
				str.Append(_dictionary[s].ToString(indent));
			}
			for(var i=str.Length-1;i>=start;i--)
				if(str[i]=='\n')
					str.Insert(i+1,indent);
			str.Append("\n}");
		}
	}

	public override Json DeepCopy(){
		var other=new JsonObject();
		foreach(var pair in this) other.Put(pair.Key,pair.Value.DeepCopy());
		return other;
	}


	public override double AsNumber()=>Length;
	public override bool AsBoolean()=>Length!=0;

	public override string AsString()=>ToString();

	public bool EqualsIgnoreOrder(JsonObject? other)=>other!=null&&other._dictionary.Count==_dictionary.Count&&!other._dictionary.Except(_dictionary).Any();

	public override bool Equals(object? obj)=>obj is JsonObject other&&other._order.SequenceEqual(_order)&&EqualsIgnoreOrder(other);

	public override int GetHashCode()=>_dictionary.GetHashCode();
}