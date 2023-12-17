using JetBrains.Annotations;
using PlayifyUtility.Windows.Win.Native;

namespace PlayifyUtility.Windows.Utils;

[PublicAPI]
public static class WindowsTypeExtensions{
	public static void Deconstruct(this NativePoint point,out int x,out int y){
		x=point.X;
		y=point.Y;
	}

	public static void Deconstruct(this Point point,out int x,out int y){
		x=point.X;
		y=point.Y;
	}

	public static void Deconstruct(this PointF point,out float x,out float y){
		x=point.X;
		y=point.Y;
	}

	public static void Deconstruct(this Size point,out int w,out int h){
		w=point.Width;
		h=point.Height;
	}

	public static void Deconstruct(this SizeF point,out float w,out float h){
		w=point.Width;
		h=point.Height;
	}
}