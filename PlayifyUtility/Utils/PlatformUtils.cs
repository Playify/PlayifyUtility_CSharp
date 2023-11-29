using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace PlayifyUtility.Utils;

[PublicAPI]
public partial class PlatformUtils{
	public static bool IsAndroid()=>Type.GetType("Android.Content.Context")!=null;
	public static bool IsLinux()=>RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
	public static bool IsWindows()=>RuntimeInformation.IsOSPlatform(OSPlatform.Windows);


	public static async ValueTask<PhysicalAddress?> GetMac(IPAddress ip){
		if(IsAndroid()){
			var process=Process.Start(new ProcessStartInfo("su","-c cat /proc/net/arp"){
				UseShellExecute=false,RedirectStandardOutput=true,CreateNoWindow=true
			});
			if(process==null) throw new IOException("Error getting MAC Address from "+ip+" (Error starting Process)");
			var s=await process.StandardOutput.ReadToEndAsync();
			foreach(var line in s.Split('\n')){
				if(!line.StartsWith(ip.ToString())) continue;
				var regex=new Regex("(?:[0-9a-f]{2}:){5}[0-9a-f]{2}");
				var match=regex.Match(line);
				if(match.Success) return PhysicalAddress.Parse(match.Groups[0].Value.ToUpperInvariant().Replace(':','-'));
				throw new IOException("Error getting MAC Address from "+ip+" (Invalid Format)");
			}
			return null;
		} else{
			var process=Process.Start(new ProcessStartInfo("arp","-a "+ip){
				UseShellExecute=false,RedirectStandardOutput=true,CreateNoWindow=true
			});
			if(process==null) throw new IOException("Error getting MAC Address from "+ip+" (Error starting Process)");
			var s=await process.StandardOutput.ReadToEndAsync();
			var regex=new Regex("(?:[0-9a-f]{2}-){5}[0-9a-f]{2}");
			var match=regex.Match(s);
			if(match.Success) return PhysicalAddress.Parse(match.Groups[0].Value.ToUpperInvariant().Replace(':','-'));
			return null;
		}
	}

	public static async Task WakeOnLan(IPAddress ip,PhysicalAddress mac){
		var macBytes=mac.GetAddressBytes();
		if(macBytes.Length!=6) throw new Exception("Illegal MAC Address: "+mac);
		using var client=new UdpClient();
		client.Connect(ip,9);

		var bytes=new byte[6+16*macBytes.Length];
		for(var i=0;i<6;i++) bytes[i]=0xff;
		for(var i=6;i<bytes.Length;i+=macBytes.Length) macBytes.CopyTo(bytes,i);

		await client.SendAsync(bytes,bytes.Length);
	}
}