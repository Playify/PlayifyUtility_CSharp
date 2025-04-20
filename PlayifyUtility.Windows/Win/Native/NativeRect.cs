using System.Runtime.InteropServices;

namespace PlayifyUtility.Windows.Win.Native;

[StructLayout(LayoutKind.Sequential)]
public struct NativeRect{
	public int Left;
	public int Top;
	public int Right;
	public int Bottom;

	public int X=>Left;
	public int Y=>Top;
	public int Width=>Right-Left;
	public int Height=>Bottom-Top;

	public override string ToString()=>$"{nameof(NativeRect)}({Left},{Top})->({Right},{Bottom})";

	public static implicit operator Rectangle(NativeRect rect)=>Rectangle.FromLTRB(rect.Left,rect.Top,rect.Right,rect.Bottom);

	public static implicit operator NativeRect(Rectangle rect)
		=>new(){
			Top=rect.Top,
			Bottom=rect.Bottom,
			Left=rect.Left,
			Right=rect.Right,
		};

	public NativePoint TopLeft=>new(){X=Left,Y=Top};
	public NativePoint BottomRight=>new(){X=Right,Y=Bottom};
	public Point Location=>new(Left,Top);
	public Size Size=>new(Width,Height);
}