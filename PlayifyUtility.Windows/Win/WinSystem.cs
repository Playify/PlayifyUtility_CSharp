using JetBrains.Annotations;

namespace PlayifyUtility.Windows.Win;

[PublicAPI]
public static partial class WinSystem{
	public static int KeyboardDelay{//Range from 0-3 (0=250ms, 3=1sec)
		get=>SystemParametersInfo(0x0016,0,out var delay,0)
			     ?delay
			     :throw new Exception("Failed to retrieve keyboard repeat delay.");
		set{
			if(!SystemParametersInfo(0x0017,value,0,0)) throw new Exception("Failed to set keyboard repeat delay.");
		}
	}

	public static bool IsSystemShuttingDown=>GetSystemMetrics(0x2000)!=0;
}