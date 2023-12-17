using System.Diagnostics.CodeAnalysis;
using PlayifyUtility.Utils.Extensions;

namespace PlayifyUtility.Jsons;

public abstract partial class Json{
	protected static bool TryParseGeneric<T>(string s,[MaybeNullWhen(false)]out T json,Func<TextReader,T?> func) where T:Json{
		using var reader=new StringReader(s);
		if(!func(reader).NotNull(out json)) return false;
		return NextPeek(reader)==-1;//Check that nothing is afterwards
	}

	protected static bool TryParseGeneric<T>(ref string s,[MaybeNullWhen(false)]out T json,Func<TextReader,T?> func) where T:Json{
		using var reader=new StringReader(s);
		if(!func(reader).NotNull(out json)) return false;
		// ReSharper disable once AssignNullToNotNullAttribute
		s=reader.ReadToEnd();
		return true;
	}


	protected static int NextRead(TextReader r){
		while(true){
			var c=r.Read();
			if(c=='/')
				if(!SkipComment(r))
					return -1;//Error
			if(!IsWhitespace(c)) return c;
		}
	}

	protected static int NextPeek(TextReader r){
		while(true){
			var c=r.Peek();
			if(c=='/'){
				r.Read();
				if(!SkipComment(r)) return -1;//Error
				continue;
			}
			if(!IsWhitespace(c)) return c;
			r.Read();
		}
	}

	private static bool SkipComment(TextReader r){
		var read=r.Read();
		switch(read){
			case '*':
				var c=r.Read();
				while(true)
					if(c==-1) return false;
					else if(c=='*'){
						c=r.Read();
						if(c=='/') return true;
					} else c=r.Read();
			case '/':
				while(true){
					switch(r.Read()){
						case '\r':
						case '\n':
							return true;
						case -1:
							return false;
					}
				}
			default:return false;
		}
	}

	private static bool IsWhitespace(int c)=>c is ' ' or '\r' or '\n' or '\t';
}