using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using PlayifyUtility.Utils.Extensions;

namespace PlayifyUtility.Utils;

[PublicAPI]
public static class FunctionUtils{
	public static bool TryGetAlways<T>(T value,[MaybeNullWhen(false)]out T t){
		t=value;
		return true;
	}

	public static bool TryGetNever<T>([MaybeNullWhen(false)]out T t){
		t=default;
		return false;
	}

	public static T RunThenReturn<T>(Action a,T returnValue){
		a();
		return returnValue;
	}

	public static void Ignore<T>(T toIgnore){}


	[DoesNotReturn]
	public static void SleepForever(){
		while(true) Thread.Sleep(Timeout.Infinite);
		// ReSharper disable once FunctionNeverReturns
	}

	[DoesNotReturn]
	public static void SleepForever(CancellationToken cancel){
		while(true){
			cancel.ThrowIfCancellationRequested();
			cancel.WaitHandle.WaitOne();
		}
		// ReSharper disable once FunctionNeverReturns
	}

	public static Task DelayForever()=>Task.Delay(Timeout.Infinite);
	public static Task DelayForever(CancellationToken cancel)=>Task.Delay(Timeout.Infinite,cancel);


	public static void Retry(Action func,int tries=3,TimeSpan? delay=null){
		var tryNumber=0;
		while(true)
			try{
				func();
				return;
			} catch(Exception) when(tryNumber++<tries){
				if(delay.TryGet(out var d)) Thread.Sleep(d);
			}
	}

	public static T Retry<T>(Func<T> func,int tries=3,TimeSpan? delay=null){
		var tryNumber=0;
		while(true)
			try{
				return func();
			} catch(Exception) when(tryNumber++<tries){
				if(delay.TryGet(out var d)) Thread.Sleep(d);
			}
	}

	public static T? RetryOrDefault<T>(Func<T> func,int tries=3,TimeSpan? delay=null){
		try{
			return Retry(func,tries,delay);
		} catch(Exception){
			return default;
		}
	}

	public static async Task RetryAsync(Func<Task> func,int tries=3,TimeSpan? delay=null){
		var tryNumber=0;
		while(true)
			try{
				await func();
				return;
			} catch(Exception) when(tryNumber++<tries){
				if(delay.TryGet(out var d)) await Task.Delay(d);
			}
	}

	public static async Task<T> RetryAsync<T>(Func<Task<T>> func,int tries=3,TimeSpan? delay=null){
		var tryNumber=0;
		while(true)
			try{
				return await func();
			} catch(Exception) when(tryNumber++<tries){
				if(delay.TryGet(out var d)) await Task.Delay(d);
			}
	}

	public static async Task<T?> RetryOrDefaultAsync<T>(Func<Task<T>> func,int tries=3,TimeSpan? delay=null){
		try{
			return await Retry(func,tries,delay);
		} catch(Exception){
			return default;
		}
	}
}

public delegate bool TryParseFunction<in TSource,TResult>(TSource source,out TResult result);