using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace PlayifyUtility.Windows.Win.Native;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
[StructLayout(LayoutKind.Sequential)]
public struct NativeColor(Color color){
	private readonly byte R=color.R;
	private readonly byte G=color.G;
	private readonly byte B=color.B;
	private readonly byte A=0;

	public uint GetRgb()=>(uint) ((R<<16)|(G<<8)|B);

	public static implicit operator Color(NativeColor color)=>Color.FromArgb(color.A,color.R,color.G,color.B);
	public static implicit operator NativeColor(Color color)=>new(color);

	public override string ToString()=>$"{nameof(NativeColor)}(#{((Color)this).ToArgb():x8})";
}