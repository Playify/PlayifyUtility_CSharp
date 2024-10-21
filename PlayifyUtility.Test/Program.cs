using PlayifyUtility.Windows.Features.Interact;

namespace PlayifyUtils.Test;

internal static class Program{
	[STAThread]
	public static void Main(string[] args){
		Console.WriteLine("Starting");
		Thread.Sleep(1000);
		Console.WriteLine("Sending");

		var s=
			"""
			5	1.62E-04	
			6	4.97E-04	
			8	1.11E-03	
			12	2.02E-03	
			16	5.91E-03	
			23	5.27E-03	
			32	1.64E-03	
			44	3.69E-04	
			62	1.68E-04	
			86	7.82E-05	
			120	1.89E-04	
			167	1.25E-04	
			233	6.44E-07	
			325	5.73E-07	
			453	5.96E-06	
			633	6.35E-06	
			882	9.47E-06	
			1231	7.36E-06	
			1717	1.70E-06	
			2000	1.70E-06	
			""";
		var send=new Send();

		foreach(var c in s){
			send.Text(c.ToString());
			send.Wait(10);
		}

		send.SendNow();


		Application.Run();
	}
}