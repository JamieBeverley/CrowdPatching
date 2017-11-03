using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityOSC;


public class TextScript : MonoBehaviour {

	public TextMesh textMesh;
	private OSCServer server;
	private OSCServer client;
	private float coherence;
	private float motion;
	private string textHack;
	public static int textWidth = 46;
	public static int maxLines = 28;
	// Use this for initialization
	void Start () {
//		coherence = 0;
//		motion = 0;
//		textMesh = GetComponent<TextMesh>();
//		textMesh.text = "oiieea??";
//		textHack = "";
//		print ("OsC initialized");
//		server = new OSCServer (9003);
//		server.PacketReceivedEvent += OnPacketReceived
//		textMesh.text = "Test";
	}

//	void OnPacketReceived(OSCServer oscServer, OSCPacket packet){
//		//		text.text = "ping";
//	
//		if (packet.Address == "/text") {
//			textHack = (packet.Data [0].ToString ());
////			textMesh.text = (packet.Data[0]).ToString();
//		} else if (packet.Address == "/coherence") {
//			coherence = float.Parse (packet.Data [0].ToString ());
//		} else if (packet.Address == "/motion") {
//			motion = float.Parse (packet.Data [0].ToString ());
//			print ("osc received from node Server");
//		}
//
//	}

	// Update is called once per frame
	void Update () {
//		textMesh.text=textHack;
//		transform.position = transform.position + (new Vector3(motion,motion,motion));
	}
}
