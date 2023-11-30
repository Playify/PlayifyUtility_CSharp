using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using JetBrains.Annotations;
using PlayifyUtility.Utils;
using PlayifyUtility.Utils.Extensions;

namespace PlayifyUtility.Jsons;

[PublicAPI]
public class JsonArray:Json,IEnumerable<Json>{
	private readonly List<Json> _value=new();
	public JsonArray(){}
	public JsonArray(IEnumerable<Json> e){_value.AddRange(e);}
	public JsonArray(IEnumerable<double> e):this(e.Select(el=>(Json)el)){}
	public JsonArray(IEnumerable<int> e):this(e.Select(el=>(Json)el)){}
	public JsonArray(IEnumerable<long> e):this(e.Select(el=>(Json)el)){}
	public JsonArray(IEnumerable<bool> e):this(e.Select(el=>(Json)el)){}
	public JsonArray(IEnumerable<string> e):this(e.Select(el=>(Json)el)){}
	
	#region Parse
	public static bool TryParse(string s,[MaybeNullWhen(false)]out JsonArray json)=>TryParseGeneric(s,out json,ParseOrNull);
	public static bool TryParse(ref string s,[MaybeNullWhen(false)]out JsonArray json)=>TryParseGeneric(ref s,out json,ParseOrNull);
	public static bool TryParse(TextReader s,[MaybeNullWhen(false)]out JsonArray json)=>ParseOrNull(s).NotNull(out json);


	public new static JsonArray? ParseOrNull(string s)=>TryParse(s,out var json)?json:null;
	public new static JsonArray? ParseOrNull(ref string s)=>TryParse(ref s,out var json)?json:null;
	public new static JsonArray? ParseOrNull(TextReader r){
		if(NextRead(r)!='[') return null;
		var o=new JsonArray();
		var c=NextPeek(r);
		switch(c){
			case ',':
				r.Read();
				return NextRead(r)==']'?o:null;
			case ']':{
				r.Read();
				return o;
			}
		}
		while(true){
			if(!Json.TryParse(r,out var child)) return null;
			o.Add(child);
			c=NextRead(r);
			if(c==']') return o;
			if(c!=',') return null;
			c=NextPeek(r);
			if(c!=']') continue;
			r.Read();
			return o;
		}
	}
	#endregion

	#region Convert

	public override Json DeepCopy(){
		var o=new JsonArray();
		foreach(var value in this) o.Add(value.DeepCopy());
		return o;
	}

	public override JsonArray AsArray()=>this;

	public override string ToString(string? indent)=>Append(new StringBuilder(),indent).ToString();

	public override StringBuilder Append(StringBuilder str,string? indent){
		if(Count==0) return str.Append("[]");
		if(indent==null){
			str.Append('[');
			var first=true;
			foreach(var child in _value){
				if(first) first=false;
				else str.Append(',');
				child.Append(str,null);
			}
			return str.Append(']');
		} else{
			str.Append("[\n");
			var start=str.Length-1;
			var first=true;
			foreach(var child in _value){
				if(first) first=false;
				else str.Append(",\n");
				child.Append(str,indent);
			}
			for(var i=str.Length-1;i>=start;i--)
				if(str[i]=='\n')
					str.Insert(i+1,indent);
			return str.Append("\n]");
		}
	}
	#endregion

	#region Operators
	public override bool Equals(object? obj)=>obj is JsonArray other&&other._value.SequenceEqual(_value);
	public override int GetHashCode()=>_value.GetHashCode();
	
	public IEnumerator<Json> GetEnumerator()=>_value.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator()=>GetEnumerator();
	#endregion
	
	#region Accessor
	public override int Count=>_value.Count;
	
	[AllowNull]
	public override Json this[int index]{
		get=>_value[index];
		set=>_value[index]=value??JsonNull.Null;
	}
	public override bool TryGet(int index,[MaybeNullWhen(false)]out Json json){
		if(index<0||index>=Count) return FunctionUtils.TryGetNever(out json);
		json=_value[index];
		return true;
	}

	public void Add(Json? json)=>_value.Add(json??JsonNull.Null);
	public void Insert(int index,Json? json)=>_value.Insert(index,json??JsonNull.Null);
	public void Remove(Json? json)=>_value.Remove(json??JsonNull.Null);
	public void RemoveAt(int index)=>_value.RemoveAt(index);

	#endregion
}