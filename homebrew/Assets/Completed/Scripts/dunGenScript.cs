using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;	
using UnityEngine.EventSystems;

namespace Completed {
	public class dunGenScript : MonoBehaviour {
		private GameObject dunGen;
		private GameObject nrMin;
		private List<GameObject> dunGenList;
		private float centerX;
		private float centerY;

		void Awake() {
			dunGen = GameObject.Find ("dunGen");
			dunGenList = new List<GameObject> ();
			dunGenList.Add(GameObject.Find ("dunGenType"));
			dunGenList.Add(GameObject.Find ("dunGenMinRooms"));
			dunGenList.Add(GameObject.Find ("dunGenMaxRooms"));
			dunGenList.Add(GameObject.Find ("dunGenGenerate"));
			centerX = dunGen.transform.position.x;
			centerY = dunGen.transform.position.y;
			float listY = centerY + 200;
			for (var i = 0; i < 4; i++) {
				dunGenList [i].transform.position = new Vector2 (centerX, listY -= 30);
			}
		}

		void Update() {
			for (var i = 0; i < 4; i++) {
				ButtonStuff bs = dunGenList [i].GetComponent<ButtonStuff> ();
				Text t = dunGenList[i].GetComponent<Text> ();
				t.text = bs.buttonString;
			}				
		}
	}
}
