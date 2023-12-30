
using JetBrains.Annotations;

namespace PlayifyUtility.Windows.Features.Hooks;

public delegate void MouseEventHandler(MouseEvent e);

[PublicAPI]
public class MouseEvent{
	public readonly int X;
	public readonly int Y;

	public readonly int Delta;
	public readonly MouseButtons Button;

	public MouseEvent(int x,int y,MouseButtons button,int delta=0){
		X=x;
		Y=y;
		Button=button;
		Delta=delta;
	}

	public bool Handled{get;set;}

	public Keys Key
		=>Button switch{
			MouseButtons.Left=>Keys.LButton,
			MouseButtons.Middle=>Keys.MButton,
			MouseButtons.Right=>Keys.RButton,
			MouseButtons.XButton1=>Keys.XButton1,
			MouseButtons.XButton2=>Keys.XButton2,
			_=>Keys.None,
		};
}