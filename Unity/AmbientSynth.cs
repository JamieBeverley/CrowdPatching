using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityOSC;

public class AmbientSynth : MonoBehaviour {


	//	Excitement:
	//		Speed of objects
	//		Rotation/animation of game objects
	//		Collision impacts and object deaths
	//		Shading - color, opacity, brightness
	// Diversity:
	//		Spawn position
	//		Color
	//		particles
	//		different depths?
	//		new colors?

	// Interactivity parameters
	//		What can people do to modify visuals and audio - including what can they do to change those hi level params?
	//			

	public OSC_Script oscController;

	public float expireTime;
	public float excitement;
	public float amp;
	public OSCClient oscClient;
	private float instantiateTime;
	private ParticleSystem ps;
	public Color col;
	public bool colorChange;

	public AmbientSynth(float expT, float excite,float amp){
		expireTime = expT;
		excitement = excite;
		this.amp = amp;
	}

	// Use this for initialization
	void Start () {
//		col = new Color (0, 255, 244, 255);
//		colorChange = false;
		ps = GetComponent<ParticleSystem> ();
//		Vector3 FUCK = ps.shape.box;
//		FUCK = new Vector3 (50, 3, 200);
		instantiateTime = Time.time;
		transform.localScale = transform.localScale * amp * 10;
	}

	public void setColor(Color color){
		col = color;
		colorChange = true;
	}

	// Update is called once per frame
	void Update () {
//		ps.startColor = Color.red;
		if (colorChange) {
//			ParticleSystem.MainModule f = ps.main;
//			f.startColor = Color.red;
			ps.startColor = col;
			colorChange = false;
		}

		excitement = oscController.excitement;

		if (expireTime != 0 && instantiateTime + expireTime < Time.time) {
			Destroy (gameObject);
		} else if (instantiateTime + expireTime - Time.time < 5) {
			ParticleSystem.MainModule pSMain = GetComponent<ParticleSystem> ().main;
			pSMain.loop = false;
		}else if (instantiateTime +30 < Time.time){  // Destroy if somehow the object lasts longer then 30 seconds
			Destroy (gameObject);
		}
	}

	public void setExpTime (float t){
		expireTime = t;

	}
}
