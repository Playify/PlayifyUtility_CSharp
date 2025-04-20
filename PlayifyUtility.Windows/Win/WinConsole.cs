using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Win32.SafeHandles;
using PlayifyUtility.Windows.Features;
using PlayifyUtility.Windows.Win.Native;

namespace PlayifyUtility.Windows.Win;

[PublicAPI]
public static partial class WinConsole{
	public static WinWindow ConsoleWindow=>new(GetConsoleWindow());

	public static bool RunningInRider=>
		ConsoleWindow.Class=="PseudoConsoleWindow"&&//PseudoConsoleWindow would also be true for Win11 Terminal app
		(ConsoleWindow.ExStyle&ExStyle.NoActivate)!=0&&//RightScrollBar, Left, Transparent, ToolWindow, WindowEdge, Layered, NoActivate
		(ConsoleWindow.GwlStyle&GwlStyle.DlgFrame)!=0;//Overlapped, TabStop, Group, Thickframe, DlgFrame, ClipSiblings, PopupWindow

	public static bool Visible{
		get=>ConsoleWindow.NonZero(out var console)&&console.IsVisible;
		set{
			var window=ConsoleWindow;
			if(window==WinWindow.Zero) return;
			if(RunningInRider) return;
			window.IsVisible=value;
			if(value) window.SetForeground();
		}
	}

	public static bool EnableAnsi()=>EnableAnsi(GetStdHandle(-11))&EnableAnsi(GetStdHandle(-12));//STD_OUTPUT_HANDLE,STD_ERROR_HANDLE

	private static bool EnableAnsi(IntPtr stream)=>
		GetConsoleMode(stream,out var mode)&&
		SetConsoleMode(stream,mode|1|4);//ENABLE_PROCESSED_OUTPUT|ENABLE_VIRTUAL_TERMINAL_PROCESSING


	public static WinWindow CreateConsole(){
		if(!AllocConsole()) return ConsoleWindow;//Already has one allocated
		if(!ConsoleWindow.NonZero(out var console)) throw new SystemException("Error creating Console");//Failed to get newly allocated one

		var conIn=CreateFile("CONIN$",GenericRead,FileShareRead,0,OpenExisting,0,0);
		Console.SetIn(new StreamReader(
			new FileStream(
				new SafeFileHandle(
					conIn,
					true),
				FileAccess.Read),
			Encoding.Default));

		var conOut=CreateFile("CONOUT$",GenericWrite,FileShareWrite,0,OpenExisting,0,0);
		Console.SetOut(new StreamWriter(
			new FileStream(
				new SafeFileHandle(
					conOut,
					true),
				FileAccess.Write),
			Encoding.Default){
			AutoFlush=true,
		});
		Console.SetError(Console.Out);


		//For net48, Console has hidden fields, that need to be overridden, to enable Console.ReadKey()
		typeof(Console).GetField("_consoleInputHandle",BindingFlags.NonPublic|BindingFlags.Static)?.SetValue(null,conIn);
		typeof(Console).GetField("_consoleOutputHandle",BindingFlags.NonPublic|BindingFlags.Static)?.SetValue(null,conOut);

		//For net6, Console accesses the StdHandle every time, so it needs to be set correctly
		SetStdHandle(-10,conIn);
		SetStdHandle(-11,conOut);
		SetStdHandle(-12,conOut);
		return console;
	}


	public static WinWindow CreateHiddenConsole(){
		var console=CreateConsole();
		if(!RunningInRider) console.IsVisible=false;
		return console;
	}


	public static NotifyIcon CreateHiddenConsole(string notifyIconName,ContextMenuStrip? strip=null,bool consoleTitle=true){
		if(consoleTitle)
			try{
				Console.Title=notifyIconName;
			} catch(IOException){
			}


		var notifyIcon=new NotifyIcon{
			Icon=ShellThumbnail.GetOwnExeIcon(false)??SystemIcons.Application,
			Text=$"{notifyIconName} (Hidden)",
			ContextMenuStrip=strip??new ContextMenuStrip{
				Items={
					{"Exit",null,(_,_)=>Environment.Exit(0)},
				},
			},
			Visible=true,
		};
		AppDomain.CurrentDomain.ProcessExit+=(_,_)=>{
			notifyIcon.Visible=false;
			notifyIcon.Dispose();
		};

		if(RunningInRider) notifyIcon.Text=notifyIconName;
		else{
			var console=CreateHiddenConsole();
			notifyIcon.MouseClick+=(_,e)=>{
				if((e.Button&MouseButtons.Left)==0) return;
				var visible=console.IsVisible^=true;
				notifyIcon.Text=$"{notifyIconName} ({(visible?"Visible":"Hidden")})";
				if(visible) console.SetForeground();
			};
		}

		return notifyIcon;
	}
}