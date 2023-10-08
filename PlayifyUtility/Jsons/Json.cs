using System.Globalization;
using System.Text;
using JetBrains.Annotations;

namespace PlayifyUtility.Jsons;

[PublicAPI]
public abstract class Json{
	public override string ToString()=>ToString(null);

	public string ToString(int indentSpaces)=>ToString(indentSpaces<0?null:new string(' ',indentSpaces));

	public abstract string ToString(string? indent);
	public virtual void Append(StringBuilder str,string? indent)=>str.Append(ToString(indent));

	public abstract Json DeepCopy();

	public abstract double AsNumber();
	public abstract bool AsBoolean();
	public abstract string? AsString();
	public JsonObject? AsObject()=>this as JsonObject;
	public JsonArray? AsArray()=>this as JsonArray;

	public abstract override bool Equals(object? obj);
	public abstract override int GetHashCode();

	public static explicit operator bool(Json? j)=>j?.AsBoolean()??false;
	public static explicit operator double(Json? j)=>j?.AsNumber()??default;
	public static explicit operator string?(Json? j)=>j?.AsString();

	public static implicit operator Json(bool b)=>b?JsonBool.True:JsonBool.False;
	public static implicit operator Json(double number)=>new JsonNumber(number);
	public static implicit operator Json(int number)=>new JsonNumber(number);
	public static implicit operator Json(long number)=>new JsonNumber(number);
	public static implicit operator Json(string s)=>new JsonString(s);

	public static Json? Parse(string s){
		try{
			var reader=new StringReader(s);
			var parse=Parse(reader);
			return NextPeek(reader)!=-1?null:parse;
		} catch(Exception){
			return null;
		}
	}

	public static Json? Parse(ref string s){
		try{
			var reader=new StringReader(s);
			var parse=Parse(reader);

			s=reader.ReadToEnd();

			return parse;
		} catch(Exception){
			return null;
		}
	}

	public static Json Parse(TextReader r){
		var c=NextRead(r);
		switch(c){
			case '{':{
				var o=new JsonObject();
				c=NextRead(r);
				switch(c){
					case ',' when NextRead(r)=='}':return o;
					case ',':throw new JsonException();
					case '}':return o;
				}
				if(c!='"') throw new JsonException();
				while(true){
					var key=ContinueParseString(r);
					if(NextRead(r)!=':') throw new JsonException();
					o.Put(key,Parse(r));
					c=NextRead(r);
					if(c=='}') return o;
					if(c!=',') throw new JsonException();
					c=NextRead(r);
					if(c=='}') return o;
					if(c!='"') throw new JsonException();
				}
			}
			case '[':{
				var array=new JsonArray();
				c=NextPeek(r);
				switch(c){
					case ',':
						r.Read();
						if(NextRead(r)==']') return array;
						throw new JsonException();
					case ']':
						r.Read();
						return array;
				}
				while(true){
					array.Add(Parse(r));
					c=NextRead(r);
					if(c==']') return array;
					if(c!=',') throw new JsonException();
					c=NextPeek(r);
					if(c!=']') continue;
					r.Read();
					return array;
				}
			}
			case '"':return new JsonString(ContinueParseString(r));
			case 'n':{
				if(r.Read()!='u'||r.Read()!='l'||r.Read()!='l') throw new JsonException();
				return JsonNull.Null;
			}
			case 't':{
				if(r.Read()!='r'||r.Read()!='u'||r.Read()!='e') throw new JsonException();
				return JsonBool.True;
			}
			case 'f':{
				if(r.Read()!='a'||r.Read()!='l'||r.Read()!='s'||r.Read()!='e') throw new JsonException();
				return JsonBool.False;
			}
			default:{
				if(c is <'0' or >'9'&&c!='-') break;
				var builder=new StringBuilder();
				builder.Append((char) c);
				var allowDot=true;
				var allowE=true;
				var allowSign=false;
				while(true){
					c=r.Peek();
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
							if(!double.TryParse(builder.ToString(),NumberStyles.Any,CultureInfo.InvariantCulture,out var v)) throw new JsonException();
							return new JsonNumber(v);
						}
					}
					r.Read();//remove peeked value from stream
					allowSign=false;
				}
			}
		}
		throw new JsonException();
	}

	protected static int NextRead(TextReader r){
		while(true){
			var c=r.Read();
			if(!IsWhitespace(c)) return c;
		}
	}

	protected static int NextPeek(TextReader r){
		while(true){
			var c=r.Peek();
			if(!IsWhitespace(c)) return c;
			r.Read();
		}
	}

	private static bool IsWhitespace(int c)=>c is ' ' or '\r' or '\n' or '\t';


	internal static string ContinueParseString(TextReader r){
		var str=new StringBuilder();
		var escape=false;
		while(true){
			var c=r.Read();
			if(escape){
				switch(c){
					case '\\':
						str.Append('\\');
						break;
					case '"':
						str.Append('"');
						break;
					case '/':
						str.Append('/');
						break;
					case 'b':
						str.Append('\b');
						break;
					case 'f':
						str.Append('\f');
						break;
					case 'r':
						str.Append('\r');
						break;
					case 'n':
						str.Append('\n');
						break;
					case 't':
						str.Append('\t');
						break;
					case 'u':
						var cp=0;
						for(var i=0;i<4;i++){
							cp<<=4;
							c=r.Read();
							cp|=c switch{
								>='0' and <='9'=>c-'0',
								>='a' and <='f'=>(c-'a')+10,
								>='A' and <='F'=>(c-'A')+10,
								-1=>throw new EndOfStreamException(),
								_=>throw new JsonException(),
							};
						}
						str.Append(char.ConvertFromUtf32(cp));
						break;
					default:throw new JsonException();
				}
				escape=false;
				continue;
			}
			switch(c){
				case '"':return str.ToString();
				case -1:throw new EndOfStreamException();
				case '\\':
					escape=true;
					break;
				default:
					str.Append((char) c);
					break;
			}
		}
	}
}