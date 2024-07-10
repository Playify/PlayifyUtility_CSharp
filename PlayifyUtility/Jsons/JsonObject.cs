using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using JetBrains.Annotations;
using PlayifyUtility.Utils.Extensions;

namespace PlayifyUtility.Jsons;

[PublicAPI]
public class JsonObject:Json,IEnumerable<KeyValuePair<string,Json>>{
	private readonly Dictionary<string,Json> _dictionary=new();
	private readonly List<string> _order=new();

	public JsonObject(){}

	public JsonObject(IEnumerable<KeyValuePair<string,Json>> e){
		foreach(var (key,value) in e) Add(key,value);
	}

	public JsonObject(IEnumerable<(string key,Json value)> e){
		foreach(var (key,value) in e) Add(key,value);
	}

	#region Parse
	public static bool TryParse(string s,[MaybeNullWhen(false)]out JsonObject json)=>TryParseGeneric(s,out json,ParseOrNull);
	public static bool TryParse(ref string s,[MaybeNullWhen(false)]out JsonObject json)=>TryParseGeneric(ref s,out json,ParseOrNull);
	public static bool TryParse(TextReader s,[MaybeNullWhen(false)]out JsonObject json)=>ParseOrNull(s).NotNull(out json);


	public new static JsonObject? ParseOrNull(string s)=>TryParse(s,out var json)?json:null;
	public new static JsonObject? ParseOrNull(ref string s)=>TryParse(ref s,out var json)?json:null;

	public new static JsonObject? ParseOrNull(TextReader r){
		if(NextRead(r)!='{') return null;
		var o=new JsonObject();
		var c=NextPeek(r);
		switch(c){
			case ',':
				r.Read();
				return NextRead(r)=='}'?o:null;
			case '}':{
				r.Read();
				return o;
			}
		}
		while(true){
			if(JsonString.UnescapeOrNull(r) is not{} key) return null;
			if(NextRead(r)!=':') return null;
			if(!Json.TryParse(r,out var child)) return null;
			o[key]=child;
			c=NextRead(r);
			if(c=='}') return o;
			if(c!=',') return null;
			c=NextPeek(r);
			if(c!='}') continue;
			r.Read();
			return o;
		}
	}
	#endregion

	#region Convert
	public override Json DeepCopy(){
		var o=new JsonObject();
		foreach(var (key,value) in this) o.Add(key,value.DeepCopy());
		return o;
	}

	public override JsonObject AsObject()=>this;

	public override string ToString(string? indent)=>Append(new StringBuilder(),indent).ToString();

	public override StringBuilder Append(StringBuilder str,string? indent){
		if(Count==0) return str.Append("{}");
		if(indent==null){
			str.Append('{');
			var first=true;
			foreach(var s in _order){
				if(first) first=false;
				else str.Append(',');
				JsonString.Escape(str,s);
				str.Append(':');
				_dictionary[s].Append(str,null);
			}
			return str.Append('}');
		} else{
			str.Append("{\n");
			var start=str.Length-1;
			var first=true;
			foreach(var s in _order){
				if(first) first=false;
				else str.Append(",\n");
				JsonString.Escape(str,s);
				str.Append(':');
				_dictionary[s].Append(str,indent);
			}
			for(var i=str.Length-1;i>=start;i--)
				if(str[i]=='\n')
					str.Insert(i+1,indent);
			return str.Append("\n}");
		}
	}
	#endregion

	#region Operators
	public bool EqualsIgnoreOrder(JsonObject? other)=>other!=null&&other._dictionary.Count==_dictionary.Count&&!other._dictionary.Except(_dictionary).Any();
	public override bool Equals(object? obj)=>obj is JsonObject other&&other._order.SequenceEqual(_order)&&EqualsIgnoreOrder(other);
	public override int GetHashCode()=>_dictionary.GetHashCode();

	public IEnumerator<KeyValuePair<string,Json>> GetEnumerator()=>_dictionary.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator()=>GetEnumerator();
	#endregion

	#region Accessor
	public override int Count=>_order.Count;

	[AllowNull]public override Json this[string property]{
		get=>_dictionary[property];
		set{
			_order.Remove(property);
			_order.Add(property);
			_dictionary[property]=value??JsonNull.Null;
		}
	}
	public override bool TryGet(string property,[MaybeNullWhen(false)]out Json json)=>_dictionary.TryGetValue(property,out json);

	public override bool Has(string property)=>_dictionary.ContainsKey(property);

	public void Add(string key,Json? json){
		_dictionary.Add(key,json??JsonNull.Null);//will throw if key already exists, therefore no _order.Remove is needed
		_order.Add(key);
	}

	public bool Remove(string property){
		if(!_dictionary.Remove(property)) return false;
		_order.Remove(property);
		return true;
	}
	#endregion

}