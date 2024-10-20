using JetBrains.Annotations;
using PlayifyUtility.Utils.Extensions;

namespace PlayifyUtility.Utils;

[PublicAPI]
public static class EnumerableUtils{
	public static IEnumerable<long> Primes(){
		yield return 2;
		yield return 3;
		var longs=new List<long>{2,3};
		var curr=5L;
		while(true){
			var prime=true;
			var sqrt=Math.Sqrt(curr)+1;
			for(var i=0;i<longs.Count&&longs[i]<sqrt;i++){
				if(curr%longs[i]!=0) continue;
				prime=false;
				break;
			}
			if(prime){
				yield return curr;
				longs.Add(curr);
			}

			curr+=2;
			prime=true;
			for(var i=0;i<longs.Count&&longs[i]<sqrt;i++){
				if(curr%longs[i]!=0) continue;
				prime=false;
				break;
			}
			if(prime){
				yield return curr;
				longs.Add(curr);
			}

			curr+=4;
		}
		// ReSharper disable once IteratorNeverReturns
	}

	public static IEnumerable<long> PrimeFactors(long number){
		if(number is 0 or 1) yield break;
		var longs=Primes();
		foreach(var prime in longs){
			while(number%prime==0){
				yield return prime;
				number/=prime;
			}
			if(number==1) yield break;
		}
	}

	public static IEnumerable<long> Fibonacci(long a=1,long b=1){
		yield return a;
		yield return b;
		while(true){
			yield return a+=b;
			yield return b+=a;
		}
		// ReSharper disable once IteratorNeverReturns
	}

	public static IEnumerable<long> IndexLong(){
		var l=0L;
		while(true)
			yield return l++;
		// ReSharper disable once IteratorNeverReturns
	}

	public static IEnumerable<int> Index(){
		var l=0;
		while(true)
			yield return l++;
		// ReSharper disable once IteratorNeverReturns
	}

	public static IEnumerable<T> RepeatForever<T>(T t){
		while(true)
			yield return t;
		// ReSharper disable once IteratorNeverReturns
	}

	public static IEnumerable<T> SelectForever<T>(Func<T> selector){
		while(true)
			yield return selector();
		// ReSharper disable once IteratorNeverReturns
	}

	public static IEnumerable<T> RepeatSelect<T>(int count,Func<T> selector){
		while(count-->0)
			yield return selector();
	}

	public static IEnumerable<T> RepeatSelect<T,TArg>(int count,Func<TArg,T> selector,TArg arg){
		while(count-->0)
			yield return selector(arg);
	}
	public static IEnumerable<T> NextFromLast<T>(T start,Func<T,T> nextFromLast){
		yield return start;
		while(true)
			yield return start=nextFromLast(start);
		// ReSharper disable once IteratorNeverReturns
	}


	public static IEnumerable<T> Concat<T>(params IEnumerable<T>[] sources)=>sources.SelectMany();
	public static IEnumerable<T> Concat<T>(IEnumerable<IEnumerable<T>> sources)=>sources.SelectMany();

	public static T[] Combine<T>(T a,T[] b){
		var arr=new T[b.Length+1];
		arr[0]=a;
		Array.Copy(b,0,arr,1,b.Length);
		return arr;
	}

	public static T[] Combine<T>(T[] a,T b){
		var arr=new T[a.Length+1];
		Array.Copy(a,arr,a.Length);
		arr[a.Length]=b;
		return arr;
	}

	public static T[] Combine<T>(T[] a,T[] b){
		var arr=new T[a.Length+b.Length];
		Array.Copy(a,arr,a.Length);
		Array.Copy(b,0,arr,a.Length,b.Length);
		return arr;
	}
}