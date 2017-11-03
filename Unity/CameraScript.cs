using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour {

	public float excitement;
	public OSC_Script osc;
//	private float randTime;
//	private float stampTime;
//	private Vector3 target;
	private Vector3 velocity = Vector3.zero;
	private bool isDodging;
	private GameObject dodgeSynth;
	private Vector3 spawnPoint;
	private Quaternion spawnRotation;
	private float dodgeDistance;
	private float cameraFollowTime;
	private bool firstDodgeFrame;
	private Vector3 dodgeToPos;
	public int shakeFrames;
	public bool shake;

	// Use this for initialization
	void Start () {
		shakeFrames = 0;
		shake = false;
		dodgeToPos = Vector3.zero;
		firstDodgeFrame = false;
		spawnPoint = transform.position;
		spawnRotation = transform.rotation;
		excitement = osc.excitement;
//		randTime = 5;
//		stampTime = Time.time;
		float randx = Random.Range (-1f, 1f);
		float randy = Random.Range (-1f, 1f);
		float randz = Random.Range (-1f, 1f);
		float transx = Mathf.Clamp (transform.position.x + excitement * randx, -10, 10);
		float transy = Mathf.Clamp (transform.position.y + excitement * randy, 1, 10);
		float transz = Mathf.Clamp (transform.position.z + excitement * randz, -35, 0);

		velocity.x = randx * 3;
		velocity.y = randy * 3;
		velocity.z = randz * 3;

		dodgeDistance = Mathf.Infinity;
		isDodging = false;
//		target = (new Vector3 (transx, transy, transz))*10;
	}
	
	// Update is called once per frame
	void Update () {
		excitement = osc.excitement;
		
		// If the camera isn't already dodging some object,
		if (!isDodging ) {
			transform.position = Vector3.Lerp (transform.position,spawnPoint, Time.deltaTime*2);

			transform.rotation = Quaternion.Lerp (transform.rotation, spawnRotation, Time.deltaTime*3);
			if (Time.time - cameraFollowTime > 2) {
				GameObject[] synthsToDodge = GameObject.FindGameObjectsWithTag ("BigSynth");
				dodgeDistance = Mathf.Infinity;
				foreach (GameObject synth in synthsToDodge) {
					Vector3 diff = synth.transform.position - transform.position;
					float curDist = diff.magnitude;
					if (curDist < dodgeDistance) {
						dodgeSynth = synth;
						dodgeDistance = curDist;
						isDodging = true;
					}
				}
			}
			firstDodgeFrame = true;


		} else {
			
			if (dodgeSynth!=null && Vector3.Distance(dodgeSynth.transform.position,transform.position)<30){
				Vector3 diff = dodgeSynth.transform.position - transform.position;
				cameraFollowTime = Time.time;
				if (firstDodgeFrame) {
					dodgeToPos = new Vector3 (transform.position.x-diff.x*10f,transform.position.y-diff.y*2,spawnPoint.z);
					print("is setting initial dodge pos");
//					dodgeToPos = new Vector3 (-5f,transform.position.y-2,spawnPoint.z);

				}


				if (diff.magnitude < 10) {
//					transform.LookAt (dodgeSynth.transform);
					if (diff.magnitude<5){
						transform.rotation = Quaternion.Lerp (transform.rotation, Quaternion.LookRotation(diff), Time.deltaTime*10);
					}else{
						transform.rotation = Quaternion.Lerp (transform.rotation, Quaternion.LookRotation(diff), Time.deltaTime*4f);

					}
					//					Vector3 transVect = new Vector3 (diff.x*2, diff.y/2, 0);
//					float timeD=2;
//					transform.position = Vector3.zero;
//					directionToTT = TT.position - AA.position; directionToTT = directionToTT.normalized;
					Vector3 directionToSynth = (dodgeSynth.transform.position - transform.position) * (-1f);
					//						transform.position = transform.position*1.1f;
					directionToSynth.z=spawnPoint.z;
					transform.position = Vector3.Lerp (transform.position, directionToSynth, Time.deltaTime);
					//						transform.position = Vector3.Lerp (spawnPoint,new Vector3(100,100,100), Time.deltaTime);//*timeD);
				}else if (diff.magnitude < 20) {
					transform.rotation = Quaternion.Lerp (transform.rotation, Quaternion.LookRotation(diff), Time.deltaTime);


				}
			} // End is dodging
			else {
				transform.position = Vector3.Lerp (transform.position,spawnPoint, Time.deltaTime*2);
//				transform.position = Vector3.MoveTowards (transform.position,spawnPoint, Time.deltaTime * 2);
				transform.rotation = Quaternion.Lerp (transform.rotation, spawnRotation, Time.deltaTime*3);
				isDodging = false;
			} 
		}
			
		if (shakeFrames<5) {
			transform.position = transform.position + new Vector3 (Random.Range(-1f,1f), Random.Range(-1f,1f), Random.Range(-1f,1f))* Mathf.PerlinNoise (Time.time, 0.0F)*3*excitement;
			shakeFrames += 1;
		} else {
			transform.position = spawnPoint;
		}
		firstDodgeFrame = false;

	}
}




//		if (stampTime + randTime < Time.time) {
//			float randx = Random.Range (-1f, 1f);
//			float randy = Random.Range (-1f, 1f);
//			float randz = Random.Range (-1f, 1f);
//			float transx = Mathf.Clamp (transform.position.x + excitement * randx, -10, 10);
//			float transy = Mathf.Clamp (transform.position.y + excitement * randy, 1, 10);
//			float transz = Mathf.Clamp (transform.position.z + excitement * randz, -35, 0);
//
//			velocity.x = randx * 3;
//			velocity.y = randy * 3;
//			velocity.z = randz * 3;
//
//			target = (new Vector3 (transx, transy, transz));
//			print (target);
////		transform.position = (new Vector3 (transx, transy, transz));
////			transform.position = Vector3.SmoothDamp (transform.position, target, ref velocity, randTime);
//			stampTime = Time.time;
//		}
//		transform.position = Vector3.Lerp (transform.position, target, Time.deltaTime/10);