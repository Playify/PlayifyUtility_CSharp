using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using PlayifyUtility.HelperClasses;

namespace PlayifyUtility.Utils.Extensions;

[PublicAPI]
public static class TypeExtensions{

	#region Numbers
	public static bool HasFlags(this int t,int flags)=>(t&flags)==flags;
	public static int WithFlags(this int t,int flags,bool? set)=>set.TryGet(out var setBool)?WithFlags(t,flags,setBool):t^flags;
	public static int WithFlags(this int t,int flags,bool set)=>set?t|flags:t&~flags;
	#endregion

	#region Strings

	public static bool RemoveFromEndOf(this string end,ref string test,bool ignoreCase=false){
		if(!test.EndsWith(end,ignoreCase?StringComparison.OrdinalIgnoreCase:StringComparison.Ordinal)) return false;
		test=test.Substring(0,test.Length-end.Length);
		return true;
	}
	public static bool RemoveFromStartOf(this string start,ref string test,bool ignoreCase=false){
		if(!test.StartsWith(start,ignoreCase?StringComparison.OrdinalIgnoreCase:StringComparison.Ordinal)) return false;
		test=test.Substring(start.Length);
		return true;
	}
	

	public static IEnumerable<string> Split(this string str,Predicate<char> controller){
		var nextPiece=0;

		for(var c=0;c<str.Length;c++){
			if(!controller(str[c])) continue;
			yield return str.Substring(nextPiece,c-nextPiece);
			nextPiece=c+1;
		}
		yield return str.Substring(nextPiece);
	}

	public static bool IsSuccess(this Match t,out Match result){
		result=t;
		return t.Success;
	}
	#endregion

	#region Collections
	public static void Deconstruct<TKey,TValue>(this KeyValuePair<TKey,TValue> pair,out TKey key,out TValue value){
		key=pair.Key;
		value=pair.Value;
	}

	public static bool TryPop<T>(this Stack<T> t,[MaybeNullWhen(false)]out T pop){
		if(t.Count==0){
			pop=default!;
			return false;
		}
		pop=t.Pop();
		return true;
	}

	public static bool TryPeek<T>(this Stack<T> t,[MaybeNullWhen(false)]out T peek){
		if(t.Count==0){
			peek=default!;
			return false;
		}
		peek=t.Peek();
		return true;
	}

	public static bool Remove<TKey,TValue>(this Dictionary<TKey,TValue> t,TKey key,[MaybeNullWhen(false)]out TValue value) where TKey : notnull{
		return t.TryGetValue(key,out value)&&t.Remove(key);
	}

	public static bool TryAdd<TKey,TValue>(this Dictionary<TKey,TValue> t,TKey key,TValue value) where TKey : notnull{
		if(t.ContainsKey(key)) return false;
		t.Add(key,value);
		return true;
	}

	public static bool TryGetValue(this NameValueCollection t,string key,[MaybeNullWhen(false)]out string result){
		result=t.Get(key)!;
		return result!=null!;
	}

	public static bool TryGetValues(this NameValueCollection t,string key,[MaybeNullWhen(false)]out string[] result){
		result=t.GetValues(key)!;
		return result!=null!;
	}

	public static IDisposable AddTemporary<T>(this ISet<T> set,T t)=>new TemporarySetValue<T>(set,t);
	#endregion

	#region Streams
	public static async Task<byte[]> ReadFullyAsync(this Stream stream,int length,CancellationToken cancel=new()){
		var bytes=new byte[length];
		await stream.ReadFullyAsync(bytes,cancel);
		return bytes;
	}

	public static async Task ReadFullyAsync(this Stream stream,byte[] array,CancellationToken cancel=new()){
		var offset=0;
		while(true){
			var i=await stream.ReadAsync(array,offset,array.Length-offset,cancel);
			if(i<=0) throw new EndOfStreamException();
			offset+=i;
			if(offset==array.Length) break;
		}
	}
	#endregion

	#region Process
	public static Task WaitForExitAsync(this Process process,
		CancellationToken cancellationToken=default){
		if(process.HasExited) return Task.CompletedTask;

		process.EnableRaisingEvents=true;
		var tcs=new TaskCompletionSource();
		process.Exited+=(_,_)=>tcs.TrySetResult();
		if(cancellationToken!=default)
			cancellationToken.Register(()=>tcs.SetCanceled(cancellationToken));

		return process.HasExited?Task.CompletedTask:tcs.Task;
	}
	#endregion

	#region C# internal
	public static TaskAwaiter GetAwaiter(this WaitHandle handle)=>handle.ToTask().GetAwaiter();

	public static Task ToTask(this WaitHandle handle){
		var tcs=new TaskCompletionSource();
		var localVariableInitLock=new object();
		lock(localVariableInitLock){
			RegisteredWaitHandle callbackHandle=null!;
			callbackHandle=ThreadPool.RegisterWaitForSingleObject(handle,
				(_,_)=>{
					tcs.SetResult();
					// ReSharper disable once AccessToModifiedClosure
					lock(localVariableInitLock) callbackHandle.Unregister(null);
				},
				null,
				Timeout.Infinite,
				true);
		}

		return tcs.Task;
	}

	public static void RunClassConstructor(this Type type)=>RuntimeHelpers.RunClassConstructor(type.TypeHandle);
	#endregion

}