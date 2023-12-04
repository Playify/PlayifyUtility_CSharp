using System.Numerics;
using System.Text;
using JetBrains.Annotations;

namespace PlayifyUtility.Utils;

[PublicAPI]
public static class MathsUtils{
	public static int Clamp(int v,int min,int max)=>v<min?min:v>max?max:v;
	public static byte Clamp(byte v,byte min,byte max)=>v<min?min:v>max?max:v;
	public static short Clamp(short v,short min,short max)=>v<min?min:v>max?max:v;
	public static long Clamp(long v,long min,long max)=>v<min?min:v>max?max:v;
	public static uint Clamp(uint v,uint min,uint max)=>v<min?min:v>max?max:v;
	public static sbyte Clamp(sbyte v,sbyte min,sbyte max)=>v<min?min:v>max?max:v;
	public static ushort Clamp(ushort v,ushort min,ushort max)=>v<min?min:v>max?max:v;
	public static ulong Clamp(ulong v,ulong min,ulong max)=>v<min?min:v>max?max:v;

	public static float Clamp(float v,float min,float max)=>v<min?min:v>max?max:v;
	public static double Clamp(double v,double min,double max)=>v<min?min:v>max?max:v;

	public static float Clamp01(float v)=>v<0?0:v>1?1:v;
	public static double Clamp01(double v)=>v<0?0:v>1?1:v;

	public static Random SharedRandom
		=>
#if NETFRAMEWORK
		Shared.Value??=new Random();
	private static readonly ThreadLocal<Random> Shared=new();
#else
		System.Random.Shared;
#endif

	public static int Random()=>SharedRandom.Next();
	public static int Random(int max)=>SharedRandom.Next(max);
	public static int Random(int min,int max)=>SharedRandom.Next(min,max);
	public static double RandomDouble()=>SharedRandom.NextDouble();
	public static void RandomBytes(byte[] buffer)=>SharedRandom.NextBytes(buffer);

	public static int SetBit(int value,int mask,bool to)=>to?value|mask:value&~mask;
	public static byte SetBit(byte value,byte mask,bool to)=>(byte)(to?value|mask:value&~mask);
	public static short SetBit(short value,short mask,bool to)=>(short)(to?value|mask:value&~mask);
	public static long SetBit(long value,long mask,bool to)=>to?value|mask:value&~mask;
	public static uint SetBit(uint value,uint mask,bool to)=>to?value|mask:value&~mask;
	public static sbyte SetBit(sbyte value,sbyte mask,bool to)=>(sbyte)(to?value|mask:value&~mask);
	public static ushort SetBit(ushort value,ushort mask,bool to)=>(ushort)(to?value|mask:value&~mask);
	public static ulong SetBit(ulong value,ulong mask,bool to)=>to?value|mask:value&~mask;


	public static BigInteger Parse(string value,string digits){
		if(string.IsNullOrEmpty(value)) return BigInteger.Zero;

		var ret=BigInteger.Zero;
		var radixInt=digits.Length;
		var radixBig=new BigInteger(radixInt);
		foreach(var c in value){
			ret*=radixBig;
			var i=digits.IndexOf(c);
			if(i<0||i>radixInt) throw new ArgumentException("Illegal Character: '"+c+"'");
			ret+=i;
		}
		return ret;
	}

	public static string ToString(BigInteger value,string digits){
		var str=new StringBuilder();
		var radix=new BigInteger(digits.Length);
		while(true){
			value=BigInteger.DivRem(value,radix,out var remainder);

			str.Insert(0,digits[(int)remainder]);
			if(value.Sign==0) return str.ToString();
		}
	}

	public static ulong ParseLong(string value,string digits){
		if(string.IsNullOrEmpty(value)) return 0;

		var ret=0UL;
		var radixInt=digits.Length;
		var radixBig=(ulong)radixInt;
		foreach(var c in value){
			ret*=radixBig;
			var i=digits.IndexOf(c);
			if(i<0||i>radixInt) throw new ArgumentException("Illegal Character: '"+c+"'");
			ret+=(ulong)i;
		}
		return ret;
	}

	public static string ToString(ulong value,string digits){
		var str=new StringBuilder();
		var radix=(ulong)digits.Length;
		while(true){
			var remainder=value%radix;
			value/=radix;

			str.Insert(0,digits[(int)remainder]);
			if(value==0) return str.ToString();
		}
	}
}