using PlayifyUtility.Streams.Data;

namespace PlayifyUtils.Test;

internal static class Program{
	[STAThread]
	public static void Main(string[] args){
		var x=new DataInputBuff([192,119,223,195]);
		var f=x.ReadFloat();

		var buff=new DataOutputBuff();
		buff.WriteFloat(f);
		Console.WriteLine(f);
	}
}