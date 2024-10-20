using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using PlayifyUtility.HelperClasses.Dispose;

namespace PlayifyUtility.Utils.Extensions;

[PublicAPI]
public static class TypeExtensions{

	#region Numbers
	public static bool HasFlags(this int t,int flags)=>(t&flags)==flags;
	public static int WithFlags(this int t,int flags,bool? set)=>set.TryGet(out var setBool)?WithFlags(t,flags,setBool):t^flags;
	public static int WithFlags(this int t,int flags,bool set)=>set?t|flags:t&~flags;
	#endregion

	#region Strings
    public static bool TryRemoveFromEndOf(this string s,string end,out string result,bool ignoreCase=false){
	    if(!s.EndsWith(end,ignoreCase?StringComparison.OrdinalIgnoreCase:StringComparison.Ordinal)){
		    result=s;
		    return false;
	    }
		result=s.Substring(0,s.Length-end.Length);
		return true;
	}

	public static bool TryRemoveFromStartOf(this string s,string start,out string result,bool ignoreCase=false){
		if(!s.StartsWith(start,ignoreCase?StringComparison.OrdinalIgnoreCase:StringComparison.Ordinal)){
			result=s;
			return false;
		}
		result=s.Substring(start.Length);
		return true;
	}
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

	[Pure]
	public static string RemoveFromEnd(this string test,string end,bool ignoreCase=false)=>RemoveFromEnd(test,end,out _,ignoreCase);

	[Pure]
	public static string RemoveFromEnd(this string test,string end,out bool removed,bool ignoreCase=false){
		// ReSharper disable once AssignmentInConditionalExpression
		if(removed=test.EndsWith(end,ignoreCase?StringComparison.OrdinalIgnoreCase:StringComparison.Ordinal))
			return test.Substring(0,test.Length-end.Length);
		return test;
	}

	[Pure]
	public static string RemoveFromStart(this string test,string start,bool ignoreCase=false)=>RemoveFromStart(test,start,out _,ignoreCase);

	[Pure]
	public static string RemoveFromStart(this string test,string start,out bool removed,bool ignoreCase=false){
		// ReSharper disable once AssignmentInConditionalExpression
		if(removed=test.StartsWith(start,ignoreCase?StringComparison.OrdinalIgnoreCase:StringComparison.Ordinal))
			return test.Substring(start.Length);
		return test;
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

	#region Semaphores

	private static SemaphoreSlimReleaser ReleaseLater(this SemaphoreSlim semaphore)=>new(semaphore);
	public static SemaphoreSlimReleaser BorrowAsync(this SemaphoreSlim semaphore){semaphore.WaitAsync();return semaphore.ReleaseLater();}
	public static SemaphoreSlimReleaser BorrowAsync(this SemaphoreSlim semaphore,CancellationToken cancel){semaphore.WaitAsync(cancel);return semaphore.ReleaseLater();}
	public static SemaphoreSlimReleaser BorrowAsync(this SemaphoreSlim semaphore,TimeSpan timeout,CancellationToken cancel=default){semaphore.WaitAsync(timeout,cancel);return semaphore.ReleaseLater();}
	public static SemaphoreSlimReleaser Borrow(this SemaphoreSlim semaphore){semaphore.Wait();return semaphore.ReleaseLater();}
	public static SemaphoreSlimReleaser Borrow(this SemaphoreSlim semaphore,CancellationToken cancel){semaphore.Wait(cancel);return semaphore.ReleaseLater();}
	public static SemaphoreSlimReleaser Borrow(this SemaphoreSlim semaphore,TimeSpan timeout,CancellationToken cancel=default){semaphore.Wait(timeout,cancel);return semaphore.ReleaseLater();}

	#endregion

	#region WaitHandle
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
	#endregion

	#region Reflection
	public static void RunClassConstructor(this Type type)=>RuntimeHelpers.RunClassConstructor(type.TypeHandle);

	

	public static IList<Type>? GetGenericTypeArguments(this InvokeMemberBinder binder)
		=>System.Type.GetType("Mono.Runtime")!=null
			  ?binder
			   .GetType()
			   .GetField("typeArguments",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static)
			   ?.GetValue(binder) as IList<Type>
			  :binder
			   .GetType()
			   .GetInterface("Microsoft.CSharp.RuntimeBinder.ICSharpInvokeOrInvokeMemberBinder")
			   ?.GetProperty("TypeArguments")
			   ?.GetValue(binder,null) as IList<Type>;
	#endregion
}