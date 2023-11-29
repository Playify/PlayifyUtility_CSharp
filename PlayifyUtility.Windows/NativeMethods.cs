using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace PlayifyUtility.Windows;

public static class NativeMethods{
	[DllImport("user32.dll")]
	internal static extern bool GetCursorPos(out Point lpPoint);

	[DllImport("user32.dll")]
	internal static extern bool SetCursorPos(int x,int y);






	[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
	[StructLayout(LayoutKind.Sequential)]
	public struct ColorRef{
		private readonly byte R;
		private readonly byte G;
		private readonly byte B;
		private readonly byte A;

		public ColorRef(Color color){
			R=color.R;
			G=color.G;
			B=color.B;
			A=0;
		}

		public uint GetRgb()=>(uint) ((R<<16)|(G<<8)|B);
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct Point{
		public int x;
		public int y;
		
		public static implicit operator System.Drawing.Point(Point point)=>new(point.x,point.y);
		public static implicit operator Point(System.Drawing.Point point)=>new(){x=point.X,y=point.Y};
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct AnimationInfo{
		public uint cbSize;
		public int iMinAnimate;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct Rect{
		public int Left;
		public int Top;
		public int Right;
		public int Bottom;

		public override string ToString()=>$"({Left},{Top})->({Right},{Bottom})";
	}

	[DllImport("User32.dll")]
	public static extern bool SystemParametersInfo(uint uiAction,uint uiParam,ref AnimationInfo pvParam,uint fWinIni);


	[DllImport("gdi32.dll",CharSet=CharSet.Auto,SetLastError=true,ExactSpelling=true)]
	internal static extern int BitBlt(IntPtr hDc,int x,int y,int nWidth,int nHeight,IntPtr hSrcDc,int xSrc,int ySrc,int dwRop);



	[DllImport("user32.dll")]
	[return:MarshalAs(UnmanagedType.Bool)]
	public static extern bool IsWindow(IntPtr hWnd);


	[Serializable,StructLayout(LayoutKind.Sequential)]
	internal struct WindowPlacement{
		public int length;
		public int flags;
		public ShowWindowCommands showCmd;
		public Point ptMinPosition;
		public Point ptMaxPosition;
		public Rectangle rcNormalPosition;
	}

	public enum ShowWindowCommands{
		Hide=0,
		Normal=1,
		Minimized=2,
		Maximized=3,
		Show=5,
	}
}