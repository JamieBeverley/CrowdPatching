using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CodeFlash : MonoBehaviour {

	public GameObject text;
	public GameObject panel;
	private float flashTime;
	private bool newFlash=false;
	// Use this for initialization
	void Start () {
				
	}

	public void textFlash (string s){
		text.GetComponent<Text> ().text = s;
		newFlash = true;
	}

	// Update is called once per frame
	void Update () {
		if (newFlash) {
			text.GetComponent<Text> ().color = new Color (255, 0, 0, 1);
			panel.GetComponent<Image> ().color = new Color (0, 255, 213, 0.7f);
			flashTime = 0;
			newFlash = false;
		}
		flashTime += Time.deltaTime;
		text.GetComponent<Text>().color = Color.Lerp(new Color(255,0,0,1),new Color(255,0,0,0),flashTime/3.5f);
		panel.GetComponent<Image>().color = Color.Lerp(new Color(0,255,213,0.7f),new Color(0,255,213,0),flashTime/2);
	}
}
