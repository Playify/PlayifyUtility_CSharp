using System.Globalization;
using System.Net;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using PlayifyUtility.Windows.Utils;

namespace PlayifyUtility.Windows.Features.Interact;

[PublicAPI]
public class SendBuilder{
	#region Static
	public static Keys StringToKey(string s)=>TryConvertStringToKey(s)??throw new ArgumentException("Invalid key: "+s);

	public static Keys? TryConvertStringToKey(string s){
		if(Enum.TryParse(s,true,out Keys key)&&
		   !(int.TryParse(s,out var number)&&number==(int)key)){//Check so that enum numbers cannot be used accidentally
			return key switch{
				Keys.Control=>Keys.LControlKey,
				Keys.ControlKey=>Keys.LControlKey,
				Keys.Shift=>Keys.LShiftKey,
				Keys.ShiftKey=>Keys.LShiftKey,
				Keys.KeyCode=>null,
				Keys.Modifiers=>null,
				Keys.Menu=>Keys.LMenu,
				_=>key,
			};
		}

		if(s.Equals("ArrowUp",StringComparison.OrdinalIgnoreCase)) return Keys.Up;
		if(s.Equals("ArrowDown",StringComparison.OrdinalIgnoreCase)) return Keys.Down;
		if(s.Equals("ArrowLeft",StringComparison.OrdinalIgnoreCase)) return Keys.Left;
		if(s.Equals("ArrowRight",StringComparison.OrdinalIgnoreCase)) return Keys.Right;
		if(s.Equals("AppsKey",StringComparison.OrdinalIgnoreCase)) return Keys.Apps;
		if(s.Equals("Esc",StringComparison.OrdinalIgnoreCase)) return Keys.Escape;
		if(s.Equals("Comma",StringComparison.OrdinalIgnoreCase)) return Keys.Oemcomma;
		if(s.Equals("Period",StringComparison.OrdinalIgnoreCase)) return Keys.OemPeriod;
		if(s.Equals("Minus",StringComparison.OrdinalIgnoreCase)) return Keys.OemMinus;
		if(s.Equals("Plus",StringComparison.OrdinalIgnoreCase)) return Keys.Oemplus;
		if(s.Equals("bs",StringComparison.OrdinalIgnoreCase)) return Keys.Back;
		if(s.Equals("\\b",StringComparison.OrdinalIgnoreCase)) return Keys.Back;
		if(s.Equals("\\n",StringComparison.OrdinalIgnoreCase)) return Keys.Return;
		if(s.Equals("\\t",StringComparison.OrdinalIgnoreCase)) return Keys.Tab;
		if(s.Equals("\\0",StringComparison.OrdinalIgnoreCase)) return (Keys)0xFF;

		if(s.Equals("Ctrl",StringComparison.OrdinalIgnoreCase)) return Keys.LControlKey;
		if(s.Equals("LCtrl",StringComparison.OrdinalIgnoreCase)) return Keys.LControlKey;
		if(s.Equals("LControl",StringComparison.OrdinalIgnoreCase)) return Keys.LControlKey;
		if(s.Equals("RCtrl",StringComparison.OrdinalIgnoreCase)) return Keys.RControlKey;
		if(s.Equals("RControl",StringComparison.OrdinalIgnoreCase)) return Keys.RControlKey;

		if(s.Equals("Alt",StringComparison.OrdinalIgnoreCase)) return Keys.LMenu;
		if(s.Equals("LAlt",StringComparison.OrdinalIgnoreCase)) return Keys.LMenu;
		if(s.Equals("RAlt",StringComparison.OrdinalIgnoreCase)) return Keys.RMenu;

		if(s.Equals("Win",StringComparison.OrdinalIgnoreCase)) return Keys.LWin;
		if(s.Equals("Windows",StringComparison.OrdinalIgnoreCase)) return Keys.LWin;
		if(s.Equals("LWindows",StringComparison.OrdinalIgnoreCase)) return Keys.LWin;
		if(s.Equals("RWindows",StringComparison.OrdinalIgnoreCase)) return Keys.RWin;

		return null;
	}

	public static string KeyToString(Keys keys){
		return keys switch{
			Keys.LMenu=>"LAlt",
			Keys.RMenu=>"RAlt",
			Keys.LControlKey=>"LCtrl",
			Keys.RControlKey=>"RCtrl",
			Keys.Escape=>"Esc",
			Keys.Oemcomma=>"Comma",
			Keys.OemPeriod=>"Period",
			Keys.OemMinus=>"Minus",
			Keys.Oemplus=>"Plus",
			(Keys)0xFF=>"\\0",
			_=>keys.ToString(),
		};
	}

	public static bool IsSingleKey(string action,out Keys key){
		if(action.StartsWith("{")&&action.EndsWith("}"))
			return TryConvertStringToKey(action.Substring(1,action.Length-2).Trim()).TryGet(out key);
		key=default;
		return false;
	}
	#endregion

	private static readonly Regex AnsiPattern=new("\u001b\\[\\d+m");
	private static readonly Regex HtmlItalic=new($"{Regex.Escape(Italic)}(.*?)(?={Regex.Escape(Reset)})");
	private static readonly Regex HtmlCyan=new($"({Regex.Escape(Cyan)})(.*?)(?=\u001b\\[\\d+m)");
	private static readonly Regex HtmlDarkCyan=new($"({Regex.Escape(DarkCyan)})(.*?)(?={AnsiPattern})");

	private readonly List<(Action<Send> apply,string str)> _tuples=new();
	public SendBuilder(){}
	public SendBuilder(string s)=>Parse(s);

	public override string ToString()=>AnsiPattern.Replace(ToConsoleString(),"");
	public string ToConsoleString()=>string.Join("",_tuples.Select(t=>t.str));

	public string ToHtmlString(){
		var s=ToConsoleString();
		s=WebUtility.HtmlEncode(s);

		s=HtmlItalic.Replace(s,"<i>$1</i>");
		s=HtmlCyan.Replace(s,"$1<em>$2</em>");
		s=HtmlDarkCyan.Replace(s,"$1<b>$2</b>");
		s=AnsiPattern.Replace(s,"");

		return s;
	}

	public Send ToSend(){
		var send=new Send();
		foreach(var tuple in _tuples) tuple.apply(send);
		return send;
	}

	public void SendNow()=>ToSend().SendNow();
	public void SendOn(SynchronizationContext ctx)=>ToSend().SendOn(ctx);
	public void SendOn(UiThread thread)=>ToSend().SendOn(thread);

	private void Add(Action<Send> apply,string console)=>_tuples.Add((apply,console));

	private const string Cyan="\u001b[96m";
	private const string DarkCyan="\u001b[36m";
	private const string Reset="\u001b[0m";
	private const string Italic="\u001b[3m";

	private const string Opening=DarkCyan+"{"+Cyan;
	private const string Closing=Reset+DarkCyan+"}"+Reset;
	private const string Plus=DarkCyan+"+"+Cyan;
	
	//TODO Mouse move, click, scroll

	public SendBuilder Parse(string s){
		if(s.StartsWith("{raw}",StringComparison.OrdinalIgnoreCase)){
			Add(send=>send.Text(s.Substring(5)),$"{Opening}{Italic}Raw{Closing}{s.Substring(5)}");
			return this;
		}

		while(s.Length!=0){
			if(s[0]!='{'){
				var nextToken=s.IndexOf('{');
				if(nextToken==-1) nextToken=s.Length;
				var part=s.Substring(0,nextToken);
				Add(send=>send.Text(part),part);
				s=s.Substring(nextToken);
				continue;
			}
			var i=s.Length<2?-1:s.IndexOf('}',2);
			if(i==-1){//Fix error instead of throwing
				Add(send=>send.Text("{"),Opening+"{"+Closing);
				s=s.Substring(1);
				continue;
			}
			var inner=s.Substring(1,i-1).Trim();
			s=s.Substring(i+1);

			if(inner.Contains('\n')&&string.IsNullOrWhiteSpace(inner)){
				Add(_=>{},Opening+Italic+"\n"+Closing);
				continue;
			}
			if(inner.StartsWith("#")){
				Add(_=>{},Opening+Italic+inner+Closing);
				continue;
			}
			var args=inner.Trim().Split(new[]{' '},StringSplitOptions.RemoveEmptyEntries);

			switch(args.Length){
				case 0:throw new Exception("Invalid state reached");
				case 1:{
					if(!ParseCombo(args[0])) goto default;
					break;
				}
				case 2:{
					switch(args[1].ToLowerInvariant()){
						case "down":{
							var maybeKey=TryConvertStringToKey(args[0]);
							if(!maybeKey.TryGet(out var key)) break;
							var keyString=KeyToString(key);
							Add(send=>send.Key(key,true),
							    Opening+keyString+" down"+Closing
							);
							break;
						}
						case "up":{
							var maybeKey=TryConvertStringToKey(args[0]);
							if(!maybeKey.TryGet(out var key)) break;
							var keyString=KeyToString(key);
							Add(send=>send.Key(key,false),
							    Opening+keyString+" up"+Closing
							);
							break;
						}
						case var arg1 when int.TryParse(arg1,out var repeat):{
							if(!ParseCombo(args[0],repeat)) goto default;
							break;
						}
						default://Fix instead of throwing
							Add(send=>send.Text("{"),Opening+"{"+Closing);
							s+=inner+"}";
							break;
					}
					break;
				}

				default://Fix instead of throwing
					Add(send=>send.Text("{"),Opening+"{"+Closing);
					s+=inner+"}";
					break;
			}
		}
		return this;
	}

	private static ModifierKeys ModsOf(ref string s){
		var i=s.IndexOf('+');
		if(i==-1) return 0;
		var mod=s.Substring(0,i).ToLowerInvariant() switch{
			"shift"=>ModifierKeys.Shift,
			"ctrl"=>ModifierKeys.Control,
			"control"=>ModifierKeys.Control,
			"win"=>ModifierKeys.Windows,
			"windows"=>ModifierKeys.Windows,
			"alt"=>ModifierKeys.Alt,
			"altgr"=>ModifierKeys.AltGr,
			_=>throw new ArgumentException("Illegal Modifier Key: "+s.Substring(0,i)),
		};
		s=s.Substring(i+1);
		var other=ModsOf(ref s);
		if((other&mod)!=0) throw new ArgumentException("Duplicate Modifier Key: "+mod);
		return mod|other;
	}

	private bool ParseCombo(string arg,int repeat=1){
		var mods=ModsOf(ref arg);
		if(!TryConvertStringToKey(arg).TryGet(out var key))
			if(new StringInfo(arg).LengthInTextElements==1){
				Add(
					send=>send.Text(repeat!=1?string.Join("",Enumerable.Repeat(arg,repeat)):arg),
					Opening+arg+(repeat!=1?" "+repeat:"")+Closing);
				return true;
			} else return false;

		var parts=new List<string>();
		var altGr=(mods&ModifierKeys.AltGr)==ModifierKeys.AltGr;
		if(!altGr&&(mods&ModifierKeys.Control)!=0) parts.Add("Ctrl");
		if((mods&ModifierKeys.Shift)!=0) parts.Add("Shift");
		if(altGr) parts.Add("AltGr");
		else if((mods&ModifierKeys.Alt)!=0) parts.Add("Alt");
		if((mods&ModifierKeys.Windows)!=0) parts.Add("Win");
		parts.Add(KeyToString(key));


		Add(
			send=>send.Combo(mods,key,repeat),
			Opening+string.Join(Plus,parts)+(repeat!=1?" "+repeat:"")+Closing
		);
		return true;
	}
}