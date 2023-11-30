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
	public static bool HasFlags(this int t,int flags)=>(t&flags)==flags;
	public static int WithFlags(this int t,int flags,bool? set)=>set.TryGet(out var setBool)?WithFlags(t,flags,setBool):t^flags;
	public static int WithFlags(this int t,int flags,bool set)=>set?t|flags:t&~flags;

	public static IEnumerable<string> Split(this string str,Predicate<char> controller){
		var nextPiece=0;

		for(var c=0;c<str.Length;c++){
			if(!controller(str[c])) continue;
			yield return str.Substring(nextPiece,c-nextPiece);
			nextPiece=c+1;
		}
		yield return str.Substring(nextPiece);
	}


	public static void Deconstruct<TKey,TValue>(this KeyValuePair<TKey,TValue> pair,out TKey key,out TValue value){
		key=pair.Key;
		value=pair.Value;
	}

	public static bool TryPop<T>(this Stack<T> t,[MaybeNullWhen(false)]out T pop){
#if NETFRAMEWORK
		if(t.Count==0){
			pop=default!;
			return false;
		}
		pop=t.Pop();
		return true;
#else
		return t.TryPop(out pop);
#endif
	}

	public static bool TryPeek<T>(this Stack<T> t,[MaybeNullWhen(false)]out T peek){
#if NETFRAMEWORK
		if(t.Count==0){
			peek=default!;
			return false;
		}
		peek=t.Peek();
		return true;
#else
		return t.TryPeek(out peek);
#endif
	}

	public static bool TryGetValue(this NameValueCollection t,string key,[MaybeNullWhen(false)]out string result){
		result=t.Get(key)!;
		return result!=null!;
	}

	public static bool TryGetValues(this NameValueCollection t,string key,[MaybeNullWhen(false)]out string[] result){
		result=t.GetValues(key)!;
		return result!=null!;
	}

	public static async Task<byte[]> ReadFullyAsync(this Stream stream,int length,CancellationToken cancel=new()){
		var bytes=new byte[length];
		await ReadFullyAsync(stream,bytes,cancel);
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

	public static Task WaitForExitAsync(this Process process,
	                                    CancellationToken cancellationToken=default){
		if(process.HasExited) return Task.CompletedTask;

		process.EnableRaisingEvents=true;
#if NETFRAMEWORK
		var tcs=new TaskCompletionSource<VoidType>();
		process.Exited+=(_,_)=>tcs.TrySetResult(default);
		if(cancellationToken!=default) cancellationToken.Register(()=>tcs.SetCanceled());
#else
		var tcs=new TaskCompletionSource();
		process.Exited+=(_,_)=>tcs.TrySetResult();
		if(cancellationToken!=default)
			cancellationToken.Register(()=>tcs.SetCanceled(cancellationToken));
#endif

		return process.HasExited?Task.CompletedTask:tcs.Task;
	}

	public static bool IsSuccess(this Match t,out Match result){
		result=t;
		return t.Success;
	}

	public static IDisposable AddTemporary<T>(this HashSet<T> set,T t){
		set.Add(t);
		return new CallbackAsDisposable(()=>set.Remove(t));
	}


	public static TaskAwaiter GetAwaiter(this WaitHandle handle)=>handle.ToTask().GetAwaiter();

	public static Task ToTask(this WaitHandle handle){
#if NETFRAMEWORK
		var tcs=new TaskCompletionSource<VoidType>();
#else
		var tcs=new TaskCompletionSource();
#endif
		var localVariableInitLock=new object();
		lock(localVariableInitLock){
			RegisteredWaitHandle callbackHandle=null!;
			callbackHandle=ThreadPool.RegisterWaitForSingleObject(handle,
			                                                      (_,_)=>{
#if NETFRAMEWORK
				                                                      tcs.SetResult(default);
#else
				                                                      tcs.SetResult();
#endif
				                                                      // ReSharper disable once AccessToModifiedClosure
				                                                      lock(localVariableInitLock) callbackHandle.Unregister(null);
			                                                      },
			                                                      null,
			                                                      Timeout.Infinite,
			                                                      true);
		}

		return tcs.Task;
	}
}