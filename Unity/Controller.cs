using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Controller : MonoBehaviour {
	private Rigidbody rb;
	public float speed;
	private int count;
	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody>();
		count = 0;
	}
	
	// Update is called once per frame
	void Update () {

		//OSCMessage msg = new OSCMessage("/test", 1234);
		// client.Send(msg);
	}

	// Called before every physics calculation
	void FixedUpdate(){
		float moveHorizontal = Input.GetAxis("Horizontal");
		float moveVertical = Input.GetAxis("Vertical");
		rb.AddForce(new Vector3(moveHorizontal, 0,moveVertical)*speed);

	}

	//'other' is the object that was collided with
	void OnTriggerEnter(Collider other){
//		Destroy (other.gameObject);
		if (other.gameObject.CompareTag ("PickUp")) {
//			Destroy(other.gameObject);
			other.gameObject.SetActive(false);
			count = count + 1; 
		}
	}

}
