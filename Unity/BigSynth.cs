using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityOSC;

public class BigSynth : MonoBehaviour {


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

	private Color color;
	private bool colorChange;
	private bool collisionColorChange;
	private Color collisionColor;

	private bool collided;
	//	public static SynthAvatarScript  Create (float expT){
	//		GameObject obj = Instantiate (this) as GameObject;
	//
	//
	//		return ;
	//	}
	//

	public void setCollisionColor(Color col){
		collisionColor = col;
		collisionColorChange = true;
	}

	public void setColor(Color col){
		colorChange = true;
		color = col;
	}



	public void setVelocity(Vector3 v){
		if (rb==null){
			rb = GetComponent<Rigidbody>();
		}
		rb.velocity = v * (7f*excitement + 0.9f)*(amp*4f);
	}

	public Vector3 getVelocity ()
	{
		return rb.velocity; 
	}

	public BigSynth (float expT, float excite,float amp){
		expireTime = expT;
		excitement = excite;
		this.amp = amp;
	}

	// Use this for initialization
	void Start () {

		rb = GetComponent<Rigidbody> ();
		instantiateTime = Time.time;
		collided = false;
		particleTrigger = false;
		// Don't do this 'safety' here: the default will override the setter, because instantiate
		// 		is called on the next frame (not immediately)
		//		this.expireTime = 10; // Default expire time if for some reason it doesn't get set on instantiation
		amp = 0;
	}

	// Update is called once per frame
	void Update () {

		if (colorChange) {
			GetComponent<MeshRenderer> ().material.color = color;

			foreach (MeshRenderer mesh in gameObject.GetComponentsInChildren<MeshRenderer>()) {
				mesh.material.color = color;
			}
			colorChange = false;
		}

		if (collisionColorChange) {
			collisionParticles.GetComponent<ParticleSystem> ().startColor = collisionColor;
			collisionColorChange = false;
		}


		Quaternion rot = new Quaternion ();
		rot.SetLookRotation(new Vector3(rb.velocity.x,rb.velocity.y*(-1),rb.velocity.z));
//		rot.SetLookRotation(rb.velocity);

		transform.rotation = rot;

		excitement = oscController.excitement;
		GetComponent<BoxCollider>().material.bounciness = excitement;
		rb.AddForce (((magnet.transform.position - transform.position) * (20*excitement)) * Time.smoothDeltaTime);
		if (expireTime!=0 && instantiateTime+expireTime<Time.time){
			Destroy (gameObject);
		} else if (instantiateTime +10 < Time.time){  // Destroy if somehow the object lasts longer then 10 seconds (might want this time to be longer)
			Destroy (gameObject);
		}

		if (particleTrigger) {
			//			GameObject obj = Instantiate (collisionParticles,transform.position,Quaternion.identity) as GameObject;	
			particleTrigger = false;
		}

	}

	void FixedUpdate(){

	}

	void OnCollisionEnter (Collision col){
		bool isSphere = col.collider.tag == "Sphere";

		//		float vel = col.relativeVelocity;
		particleTrigger= true;
		GameObject obj = Instantiate (collisionParticles,transform.position,Quaternion.identity) as GameObject;	
		//		obj.GetComponent<ParticleSystem>().
		Destroy (obj, (float)obj.GetComponent<ParticleSystem>().duration+5f);
		//		collisionParticles.GetComponent<ParticleSystem> ();

		if (oscClient != null && !collided && isSphere) {
			OSCMessage msg = new OSCMessage ("/test","killedSynth");
			oscClient.Send (msg);
			Destroy (gameObject);
			collided = true;
		}
	}

	public float setExpTime (float t){
		//		this.expireTime = t;
		//		expireTime = t;
		expireTime = t;
		return expireTime;
	}
}
