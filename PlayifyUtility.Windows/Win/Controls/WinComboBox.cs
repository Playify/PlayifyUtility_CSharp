using System.Collections;
using System.Text;
using JetBrains.Annotations;
using PlayifyUtility.Windows.Win.Native;

namespace PlayifyUtility.Windows.Win.Controls;

[PublicAPI]
public readonly struct WinComboBox(WinControl control):IEnumerable<string>{
	public IEnumerator<string> GetEnumerator(){
		for(var i=0;i<Count;i++)
			yield return this[i];
	}

	IEnumerator IEnumerable.GetEnumerator()=>GetEnumerator();
	public int Count=>control.SendMessage(WindowMessage.CB_GETCOUNT,0,0);

	public string this[int index]{
		get{
			if(index<0||index>=Count) throw new ArgumentOutOfRangeException(nameof(index),"Index out of range.");

			var length=control.SendMessage(WindowMessage.CB_GETLBTEXTLEN,index,0);

			if(length<=0) return string.Empty;

			var sb=new StringBuilder(length+1);
			control.SendMessage(WindowMessage.CB_GETLBTEXT,index,sb);

			return sb.ToString();
		}
	}
	public int Selected{
		get=>control.SendMessage(WindowMessage.CB_GETCURSEL,0,0);
		set=>control.SendMessage(WindowMessage.CB_SETCURSEL,value,0);
	}
	
	///Case insensitive, exact match
	public int IndexOf(string s)=>control.SendMessage(WindowMessage.CB_FINDSTRINGEXACT,-1,s);
	///Case insensitive, starts with
	public int FindIndexOf(string start)=>control.SendMessage(WindowMessage.CB_FINDSTRING,-1,start);

	///Useful when an application only enables an apply button, if the control was changed by the user, instead of programmatically
	public void SelectUsingKeyboard(int index)=>SelectUsingKeyboard(this[index]);
	///Useful when an application only enables an apply button, if the control was changed by the user, instead of programmatically
	public void SelectUsingKeyboard(string value)=>SelectUsingKeyboard(value,value[0]);
	///Useful when an application only enables an apply button, if the control was changed by the user, instead of programmatically
	public void SelectUsingKeyboard(string value,char c){
		var maxTries=Count;
		while(maxTries-->0){
			control.SendChar(c);
			if(control.Text==value)
				return;
		}
		throw new Exception("Error setting ComboBox to "+value);
	}
}