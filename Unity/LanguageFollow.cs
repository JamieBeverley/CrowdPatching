using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanguageFollow : MonoBehaviour {
	private Vector3 offset;
	public GameObject obj;

	// Use this for initialization
	void Start () {
		offset = transform.position - obj.transform.position;

	}
	
	// Update is called once per frame
	void LateUpdate () {
		transform.position = obj.transform.position + offset;

	}
}
