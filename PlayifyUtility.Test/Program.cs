using PlayifyUtility.Streams.Data;

namespace PlayifyUtils.Test;

internal static class Program{
	[STAThread]
	public static void Main(string[] args){
		var buff=new DataOutputBuff();
		buff.WriteFloat(42);
		buff.WriteShort(0xFFFF);
		buff.WriteDouble(42);

#if NET6_0
		Console.WriteLine(Convert.ToHexString(buff.ToByteArray()));
#endif
		var inp=new DataInputBuff(buff);
		Console.WriteLine(inp.ReadFloat());
		inp.ReadShort();
		Console.WriteLine(inp.ReadDouble());
	}
}