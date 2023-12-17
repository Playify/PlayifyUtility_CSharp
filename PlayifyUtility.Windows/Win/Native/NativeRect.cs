using System.Runtime.InteropServices;

namespace PlayifyUtility.Windows.Win.Native;

[StructLayout(LayoutKind.Sequential)]
public struct NativeRect{
	public int Left;
	public int Top;
	public int Right;
	public int Bottom;

	public override string ToString()=>$"({Left},{Top})->({Right},{Bottom})";

	public static implicit operator Rectangle(NativeRect rect)=>Rectangle.FromLTRB(rect.Left,rect.Top,rect.Right,rect.Bottom);

	public static implicit operator NativeRect(Rectangle rect)
		=>new(){
			Top=rect.Top,
			Bottom=rect.Bottom,
			Left=rect.Left,
			Right=rect.Right,
		};
}