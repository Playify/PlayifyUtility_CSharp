using System.Collections.Specialized;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using PlayifyUtility.HelperClasses;

namespace PlayifyUtility.Utils;

[PublicAPI]
public static class TypeExtensions{
	public static bool HasFlags(this int t,int flags)=>(t&flags)==flags;

	public static void Deconstruct<TKey,TValue>(this KeyValuePair<TKey,TValue> pair,out TKey key,out TValue value){
		key=pair.Key;
		value=pair.Value;
	}

	public static bool TryPop<T>(this Stack<T> t,/*[MaybeNullWhen(false)]*/out T pop){
		if(t.Count==0){
			pop=default!;
			return false;
		}
		pop=t.Pop();
		return true;
	}

	public static bool TryGetValue(this NameValueCollection t,string key,/*[MaybeNullWhen(false)]*/out string result){
		result=t.Get(key)!;
		return result!=null!;
	}

	public static bool TryGetValues(this NameValueCollection t,string key,/*[MaybeNullWhen(false)]*/out string[] result){
		result=t.GetValues(key)!;
		return result!=null!;
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

		var tcs=new TaskCompletionSource();
		process.EnableRaisingEvents=true;
		process.Exited+=(_,_)=>tcs.TrySetResult();
		if(cancellationToken!=default)
			cancellationToken.Register(()=>tcs.SetCanceled(cancellationToken));

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
}