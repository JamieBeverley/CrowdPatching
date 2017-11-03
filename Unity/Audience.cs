using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Audience : MonoBehaviour {

	public OSC_Script oscController;
	public string username="";
	public int id;
	private TextMesh textMesh;
	public float diversity;
	public float excitement;
	public float depth;
	public float scale; 
	private float initialRadius;
	private float colScale;
	private Vector3 nudgePos;
	public bool solo;
	private static int maxX = 30;
	private static int minX = -30;
	private static int maxZ = 50;
	private static int minZ = -5;
	private static int maxY = 30;
	private static int minY = 1;

	public Audience (int id, string s){
		username = s;
		this.id = id;
		diversity = 0;
		depth = 0;
		excitement = 0;
		scale = 0;
	}

	public Audience (int id, string uName, float scale, float diversity, float excitement, float depth){
		this.id = id;
		this.username = uName;

		this.scale = scale;
		this.diversity = diversity;
		this.excitement = excitement;
		this.depth = depth;

	}

	public void setTo(Audience a){
		this.username = a.username;

		this.scale = a.scale;
		this.diversity = a.diversity;
		this.excitement = a.excitement;
		this.depth = a.depth;		
	}

	public void nudge(Vector3 vect){
//		transform.position.x += vect.x;
//		transform.position.y += vect.y;
//		transform.position.z += vect.z;
		print("nudge:  "+vect);
//		nudgePos = new Vector3(Mathf.Clamp(vect.x,minX,maxX),Mathf.Clamp(vect.y,minY,maxY),Mathf.Clamp(vect.z,minZ,maxZ));
		nudgePos = new Vector3(Mathf.Clamp(vect.x,-30,30),Mathf.Clamp(vect.y,3,25),Mathf.Clamp(vect.z,-5,40));
	}

	public void setValues(string uName, float scale, float diversity, float excitement, float depth){
		this.username = uName;
		this.scale = scale;
		this.diversity = diversity;
		this.excitement = excitement;
		this.depth = depth;
	}

	// Use this for initialization
	void Start () {
		initialRadius = GetComponent<SphereCollider> ().radius;
		nudgePos = transform.position;
		scale = 0;
		diversity = 0;
		depth = 0;
		excitement = 0;
		solo = false;
		textMesh = gameObject.GetComponentInChildren<TextMesh> ();
		GetComponentInChildren<ParticleSystem> ();
	}

	// Update is called once per frame
	void Update () {
		scale = scale + colScale/6;
		textMesh.text = username;

//		transform.position = Vector3.Lerp (transform.position, nudgePos, Time.deltaTime * excitement * 1); 
	
		transform.position = Vector3.Lerp (transform.position, nudgePos, Time.deltaTime* Mathf.Clamp(excitement,0.1f,1)); 
		
		if (colScale != 0) {
			colScale = colScale / 1.05f;
		}

		foreach (ParticleSystem ps in GetComponentsInChildren<ParticleSystem>()) {
			ParticleSystem.MinMaxCurve mm = new ParticleSystem.MinMaxCurve ();
			ParticleSystem.MainModule main = ps.main;

			if (ps.gameObject.tag == "AudienceMainPs") {
				main.startSize = 1 + scale * 10;
				ParticleSystem.EmissionModule em = ps.emission;
				em.rateOverTime = 20 + scale * 40;
					
			} else {
				main.startSize = scale*8 + 0.3f;
				ParticleSystem.EmissionModule em = ps.emission;
				em.rateOverTime = 180 +scale*40;
			}
			ParticleSystem.VelocityOverLifetimeModule vel = ps.velocityOverLifetime;
			ParticleSystem.MinMaxCurve rate = new ParticleSystem.MinMaxCurve ();

			vel.xMultiplier = (17 + scale * 80);
			vel.yMultiplier = (17 + scale * 80);
			vel.zMultiplier = (17 + scale * 80);
//			ps.velocityOverLifetime = (17 + scale * 10);
//			rate.curveMultiplier = (scale * 10 + 17);
//			rate.curveMultiplier = (scale * 10 + 17);
//			vel.x = rate;
//			vel.y = rate;
//			vel.z = rate;
		}
		excitement = excitement * (scale + 1);
		GetComponent<SphereCollider> ().radius = initialRadius+(scale*2);

		if (solo) {
			colScale = 1.5f;
		}

	}

	void OnCollisionEnter(Collision col){
		
		if (col.gameObject.tag != "Audience") {
			Destroy (col.gameObject);
		}
		colScale = colScale + 0.12f;
	}
}
//// Get the Velocity over lifetime modult
//ParticleSystem.VelocityOverLifetimeModule snowVelocity = GameObject.Find ("Snow").GetComponent<ParticleSystem> ().velocityOverLifetime;
//
////And to modify the value
//ParticleSystem.MinMaxCurve rate = new ParticleSystem.MinMaxCurve();
//rate.constantMax = 10.0f; // or whatever value you want
//snowVelocity.x = rate;
