using System.Runtime.InteropServices;
using JetBrains.Annotations;
using PlayifyUtility.Windows.Features;
using PlayifyUtility.Windows.Win.Native;

namespace PlayifyUtility.Windows.Win;

[PublicAPI]
public static partial class WinCursor{
	#region Cursor
	public static Point CursorPos{
		get{
			if(GetCursorPos(out var point)) return point;
			throw new Exception("Error getting cursor pos");
		}
		set=>SetCursorPos(value.X,value.Y);
	}

	public static bool TryGetCursorPos(out Point point){
		var b=GetCursorPos(out var nativePoint);
		point=nativePoint;
		return b;
	}

	[DllImport("user32.dll",EntryPoint="SetCursorPos")]
	public static extern bool SetCursorPos(int x,int y);
	#endregion

	#region Color
    public static Color? GetColorUnderCursor()=>Screenshot.GetColorUnderCursor();
	[Obsolete("Moved to Screenshot class")]
    public static Color GetColorAt(Point location)=>GetColorAt(location.X,location.Y);
	[Obsolete("Moved to Screenshot class")]
    public static Color GetColorAt(NativePoint location)=>GetColorAt(location.X,location.Y);

	[Obsolete("Moved to Screenshot class")]
	public static Color GetColorAt(int x,int y)=>Screenshot.GetColorAt(x,y);
	#endregion
}