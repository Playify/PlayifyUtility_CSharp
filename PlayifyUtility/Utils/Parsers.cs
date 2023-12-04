using System.Globalization;
using System.Net;
using JetBrains.Annotations;

namespace PlayifyUtility.Utils;

[PublicAPI]
public class Parsers{
	public static bool TryParseIpEndPoint(string s,out IPEndPoint result)=>TryParseIpEndPoint(s,0,out result);
	public static bool TryParseIpEndPoint(string s,int defaultPort,out IPEndPoint result){
		var addressLength=s.Length;
		var lastColonPos=s.LastIndexOf(':');

		if(lastColonPos>0){
			if(s[lastColonPos-1]==']') addressLength=lastColonPos;
			else if(s.Substring(0,lastColonPos).LastIndexOf(':')==-1) addressLength=lastColonPos;
		}

		if(IPAddress.TryParse(s.Substring(0,addressLength),out var address)){
			var portSubstring=s.Substring(addressLength+1);

			if(!string.IsNullOrEmpty(portSubstring)&&portSubstring[portSubstring.Length-1]!=']'){
				if(uint.TryParse(portSubstring,NumberStyles.None,CultureInfo.InvariantCulture,out var port)&&port<=IPEndPoint.MaxPort){
					result=new IPEndPoint(address,(int)port);
					return true;
				}
			} else if(string.IsNullOrEmpty(portSubstring)){
				result=new IPEndPoint(address,defaultPort);// Handle cases where port is missing
				return true;
			}
		}

		result=null!;
		return false;
	}

}