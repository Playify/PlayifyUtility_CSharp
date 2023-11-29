namespace PlayifyUtility.Maths;

public static class NumberSequences{
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

	public static IEnumerable<long> Increment(){
		var l=0L;
		while(true)
			yield return l++;
		// ReSharper disable once IteratorNeverReturns
	}
}