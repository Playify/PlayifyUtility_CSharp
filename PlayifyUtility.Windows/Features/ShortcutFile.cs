using System.Text;
using JetBrains.Annotations;
using PlayifyUtility.Windows.Win.Native;

namespace PlayifyUtility.Windows.Features;

[PublicAPI]
public partial class ShortcutFile:IDisposable{//Can be used to interact with .lnk files


	public static void CreateShortcut(string lnkPath,string linkTo,string? args=null){
		using var shortcut=new ShortcutFile();
		shortcut.Path=linkTo;
		shortcut.Arguments=args??"";
		shortcut.WorkingDirectory=System.IO.Path.GetDirectoryName(linkTo)??"";
		shortcut.Save(lnkPath);
	}

	public static string GetShortcutTarget(string lnkPath){
		using var shortcut=new ShortcutFile(lnkPath);
		return shortcut.Path;
	}


	public ShortcutFile(){}
	public ShortcutFile(string shortcutPath)=>RunChecked(Persist.Load(shortcutPath,0));
	public void Save(string shortcutPath)=>RunChecked(Persist.Save(shortcutPath,true));


	#region Properties
	public string Path{
		get{
			var sb=new StringBuilder(260);
			RunChecked(_link.GetPath(sb,sb.Capacity,IntPtr.Zero,0));
			return sb.ToString();
		}
		set=>RunChecked(_link.SetPath(value));
	}

	public string WorkingDirectory{
		get{
			var sb=new StringBuilder(260);
			RunChecked(_link.GetWorkingDirectory(sb,sb.Capacity));
			return sb.ToString();
		}
		set=>RunChecked(_link.SetWorkingDirectory(value));
	}

	public string Arguments{
		get{
			var sb=new StringBuilder(260);
			RunChecked(_link.GetArguments(sb,sb.Capacity));
			return sb.ToString();
		}
		set=>RunChecked(_link.SetArguments(value));
	}

	public string Description{
		get{
			var sb=new StringBuilder(1024);
			RunChecked(_link.GetDescription(sb,sb.Capacity));
			return sb.ToString();
		}
		set=>RunChecked(_link.SetDescription(value));
	}

	public string IconLocation{
		get{
			var sb=new StringBuilder(260);
			RunChecked(_link.GetIconLocation(sb,sb.Capacity,out _));
			return sb.ToString();
		}
		set=>RunChecked(_link.SetIconLocation(value,0));
	}

	//Only valid: Normal,Maximized,ShowMinNoActivate
	public ShowWindowCommands ShowCommand{
		get{
			RunChecked(_link.GetShowCmd(out var cmd));
			return (ShowWindowCommands)cmd;
		}
		set=>RunChecked(_link.SetShowCmd((int)value));
	}

	public short Hotkey{
		get{
			RunChecked(_link.GetHotkey(out var key));
			return key;
		}
		set=>RunChecked(_link.SetHotkey(value));
	}
	#endregion

}