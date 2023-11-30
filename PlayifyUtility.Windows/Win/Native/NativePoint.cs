using System.Runtime.InteropServices;

namespace PlayifyUtility.Windows.Win.Native;


[StructLayout(LayoutKind.Sequential)]
public struct NativePoint{
	public int X;
	public int Y;
		
	public static implicit operator Point(NativePoint point)=>new(point.X,point.Y);
	public static implicit operator NativePoint(Point point)=>new(){X=point.X,Y=point.Y};
}