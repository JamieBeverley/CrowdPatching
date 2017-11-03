using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityOSC;

public class LightSynth : MonoBehaviour {


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

	public GameObject magnet;
	public GameObject collisionParticles;
	public GameObject parent;

	public OSC_Script oscController;
	public float expireTime;
	public float excitement;
	public float amp;
	public OSCClient oscClient;
	private float instantiateTime;
	private Time clock;
	private bool particleTrigger;
	private Rigidbody rb;
	private float expireTimeUpdate;
	private int counter;
	private bool collided;
	private string newString;
	private TextMesh textMesh;
	private int textUpdateRate;
	private Color color;
	private bool colorChange;
	public float speed;
	private bool collisionColorChange;
	private Color collisionColor;
	public Vector3 travelPos;
	//	public static SynthAvatarScript  Create (float expT){
	//		GameObject obj = Instantiate (this) as GameObject;
	//
	//
	//		return ;
	//	}
	//

	public void setVelocity(Vector3 v){
		if (rb==null){
//			rb = GetComponent<Rigidbody>();
		}
//		rb.velocity = v * (7f*excitement + 0.9f)*(amp*4f);
	}

	public Vector3 getVelocity ()
	{
		return rb.velocity; 
	}

	public LightSynth(float expT, float excite,float amp){
		speed = 0;
		expireTime = expT;
		excitement = excite;
		this.amp = amp;
	}


	void Awake(){
		colorChange = false;
	}
	// Use this for initialization
	void Start () {
//		colorChange = true;
//		color = GetComponent<TextMesh> ().color;
		counter = 1;
//		rb = GetComponent<Rigidbody> ();
		textUpdateRate = 1;
		instantiateTime = Time.time;
		collided = false;
		particleTrigger = false;
		travelPos = new Vector3 (transform.position.x * (-3.4f), transform.position.y, transform.position.z);
	}

	public void setColor(Color col){
		colorChange = true;
		color = col;
	}



	// Update is called once per frame
	void Update () {
		if (colorChange) {
			GetComponent<TextMesh> ().color = color;
			GetComponent<MeshRenderer>().material.SetColor("_Color",color);
			foreach (ParticleSystem ps in gameObject.GetComponentsInChildren<ParticleSystem>()) {
				if (ps.gameObject.tag != "smokeBacking") {
					ps.startColor = color;
				}
			}
			colorChange = false;
		}

		if (collisionColorChange) {
			collisionParticles.GetComponent<ParticleSystem> ().startColor = collisionColor;
			collisionColorChange = false;
		}

		newString = "";


		if (textUpdateRate > 1) {
			for (int i = 0; i < Random.Range (30, 35); i += 1) {
				newString = newString + (Random.Range (0, 2)).ToString (); 
			
			}
			GetComponent<TextMesh> ().text = newString;
			textUpdateRate = 0;
		} else {
			textUpdateRate += 1;
		}

		if (oscController != null) {
			excitement = oscController.excitement;
		} else {
			excitement = 0.5f;
		}
		transform.position = Vector3.Lerp (transform.position, travelPos, Time.deltaTime * excitement); 
//		GetComponent<CapsuleCollider>().material.bounciness = excitement;
//		rb.AddForce (((magnet.transform.position - transform.position) * (20*excitement)) * Time.smoothDeltaTime);

		if (expireTime!=0 && instantiateTime+expireTime<Time.time){
			Destroy (gameObject);
		} else if (instantiateTime +10 < Time.time){  // Destroy if somehow the object lasts longer then 10 seconds (might want this time to be longer)
			Destroy (gameObject);
		}

		
	}

	void FixedUpdate(){

	}

	void OnCollisionEnter (Collision col){

		//@
		//		float vel = col.relativeVelocity;
//		particleTrigger= true;
//		GameObject obj = Instantiate (collisionParticles,transform.position,Quaternion.identity) as GameObject;	
//		//		obj.GetComponent<ParticleSystem>().
//		Destroy (obj, (float)obj.GetComponent<ParticleSystem>().duration+5f);
//		//		collisionParticles.GetComponent<ParticleSystem> ();
//
//		if (oscClient != null && !collided && col.gameObject.tag!="LightSynth") {
//			OSCMessage msg = new OSCMessage ("/test","killedSynth");
//			oscClient.Send (msg);
//			Destroy (gameObject);
//			collided = true;
//		}
	}

	public float setExpTime (float t){
		//		this.expireTime = t;
		//		expireTime = t;
		expireTime = t;
		return expireTime;
	}
}
