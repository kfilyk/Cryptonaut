using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;	

namespace Completed {
	public class ButtonStuff: MonoBehaviour {
		public string buttonString="";
		
		// Update is called once per frame
		public void mouseEnter () {
			buttonString = "<" + buttonString + ">";
		}
		public void mouseExit () {
			buttonString = buttonString.Substring (1, buttonString.Length-2);
		}
	}
}
