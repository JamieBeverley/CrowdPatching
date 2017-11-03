using System.Collections;
using System.Collections.Generic;
//using System;
//using System.Convert;
using UnityEngine;
using UnityEngine.UI;
using UnityOSC;

public class OSC_Script : MonoBehaviour {

	private static System.Net.IPAddress scIP = new System.Net.IPAddress (new byte[] { 192, 168, 1, 125 });
	public GameObject turqouiseRaveLight;
	public GameObject purpleRaveLight;
	public GameObject greenRaveLight;
	public GameObject frontWall;

	public Text excitementText;
	public Text diversityText;
	public Text depthText;

	public GameObject codeText;
	public GameObject audience;
	public GameObject bigSynthObj;
	public GameObject lightSynthObj;
	public GameObject ambientSynthObj;
	public GameObject audienceObj;
	public GameObject camera;
	public CameraScript cameraScript;
	public GameObject evalText;
	public GameObject dust;
	private ParticleSystem dustPS;

//	private static OSCClient scOSC = new OSCClient (new System.Net.IPAddress (new byte[] { 127, 0, 0, 1 }), 9004);
	private static OSCClient scOSC = new OSCClient (scIP, 9004);


//	public CameraScript camera;

//	Excitement:
//		Speed of objects
//		Rotation/animation of game objects
//		Collision impacts and object deaths
//		Shading - color, opacity, brightness
	public float excitement;
	public float diversity;
	public float depth;
	private float lcExcitement;
	// Directly from sc
	private float lcDiversity;
	private float lcDepth;
	public float beat;
	public bool solo;
	public int soloID;
	public float soloDur;
	public float soloTime;
	public bool newSolo; //@ this is brutal, plz fix eventually...

	public static float delta = 0.01f;
	private float motion;
	private float coherence;
	private string updatedText;

	public static Color [] colors = new Color[]{new Color(255,0,0,1),new Color(0,255,0,1),new Color(0,0,255,1)};

	private OSCServer server;
	private LinkedList<OSCClient> clients;

	private TextMesh textMesh;
	private bool synthSpawnTrigger;
	private Dictionary<int,GameObject> activeAudienceMembers;
	private LinkedList<Audience> newAudienceMembers;
	private LinkedList<int> deleteAudienceMembers;
	private LinkedList<Audience> updateAudienceMembers;
	private Dictionary<int,Vector3> nudgeAudienceMembers;

	private LinkedList<SynthAvatarScript> synthAvatars;
	private LinkedList<LightSynth> lightSynths;
	private LinkedList<AmbientSynth> ambientSynths;
	private LinkedList<BigSynth> bigSynths;
	private int beatCount = 0;
	private GameObject obj;
	private float ambSynthY;
	private string evalTextString="";
	private bool newEvalText=false;



	// Use this for initialization
	void Start () {
		ambSynthY = 4;
		cameraScript = camera.GetComponent<CameraScript> ();

		server = new OSCServer (9003);
		server.PacketReceivedEvent += OnPacketReceived;
	
		excitement = 0f;
		depth = 0f;
		diversity = 0f;
		beat = 0f;

		clients = new LinkedList<OSCClient> ();

		lcDepth = 0;
		lcExcitement = 0;
		lcDiversity = 0;

		solo = false;
		soloTime = 0;
		soloDur = 0;
		newSolo = false;

		dustPS = dust.GetComponent<ParticleSystem> ();


		newAudienceMembers = new LinkedList<Audience> ();
		deleteAudienceMembers = new LinkedList<int> ();
		updateAudienceMembers = new LinkedList<Audience> ();
		nudgeAudienceMembers = new Dictionary<int,Vector3> ();
		activeAudienceMembers = new Dictionary<int,GameObject> ();
		synthAvatars = new LinkedList<SynthAvatarScript> ();
		lightSynths = new LinkedList<LightSynth> ();
		ambientSynths = new LinkedList<AmbientSynth> ();
		bigSynths = new LinkedList<BigSynth> ();

		coherence = 0f;
		motion = 0;
		textMesh = codeText.GetComponent<TextMesh>();
		updatedText = "";

//		InvokeRepeating("drawFuncs", 0f, 0.1f);

		print ("OsC initialized");
	}

	void OnPacketReceived(OSCServer oscServer, OSCPacket packet){
		print ("osc");
		if (packet.Address == "/text") {
			updatedText = packet.Data [0].ToString ();
			//			textMesh.text = (packet.Data[0]).ToString();
		} else if (packet.Address == "/updateAudience") {
			int id = System.Convert.ToInt32 (packet.Data [0]);

			if (activeAudienceMembers.ContainsKey (id)) {
				float sc = System.Convert.ToSingle (packet.Data [2]);
				float ex = System.Convert.ToSingle (packet.Data [3]);
				float div = System.Convert.ToSingle (packet.Data [4]);
				float dep = System.Convert.ToSingle (packet.Data [5]);
				//A linked list of all audienceMembers that need to be updated from this frame of receiving OSC messages
				//Prevents concurrent updates from being dropped
				updateAudienceMembers.AddFirst (new Audience (id, packet.Data [1].ToString (), sc, ex, div, dep));
				//				activeAudienceMembers[id].GetComponent<Audience>().setValues(packet.Data[0].ToString(), (float)packet.Data[1], (float)packet.Data[2],(float)packet.Data[3],(float)packet.Data[4]);
			}
		} else if (packet.Address == "/coherence") {
//			coherence = float.Parse (packet.Data [0].ToString ());
			coherence = System.Convert.ToSingle (packet.Data [0]);
		} else if (packet.Address == "/motion") {
//			motion = float.Parse (packet.Data [0].ToString ()) * 2;
			motion = System.Convert.ToSingle (packet.Data [0]);
		} else if (packet.Address == "/nudge") {
			var id = System.Convert.ToInt32 (packet.Data [0]);
			if (activeAudienceMembers.ContainsKey (id)) {
				var x = System.Convert.ToSingle (packet.Data [1]);
				var y = System.Convert.ToSingle (packet.Data [2]);
				var z = System.Convert.ToSingle (packet.Data [3]);
				print ("nudge received for id:  " + id);
				//Add to a dictionary to be updated in main thread
				nudgeAudienceMembers.Add (id, new Vector3 (x, y, z));
			}
		} else if (packet.Address == "/newAudienceMember") {
			//No idea why had to use Convert instead of regular typecasting, but maybe its safer to use this
			// in other places too...
			int id = System.Convert.ToInt32 (packet.Data [0]);
			string username = packet.Data [1].ToString ();
			Audience a = new Audience (id, username);
			newAudienceMembers.AddFirst (a);
		} else if (packet.Address == "/removeAudienceMember") {
			int id = System.Convert.ToInt32 (packet.Data [0]);
			if (activeAudienceMembers.ContainsKey (id)) {
				deleteAudienceMembers.AddFirst (id);
			}
		} else if (packet.Address == "/l3") {
			lcExcitement = System.Convert.ToSingle (packet.Data [0]);
			lcDiversity = System.Convert.ToSingle (packet.Data [1]);
			lcDepth = System.Convert.ToSingle (packet.Data [2]);
		} else if (packet.Address == "/scsynth/lightSynth") {
			float sus = System.Convert.ToSingle (packet.Data [0]);
			float amp = System.Convert.ToSingle (packet.Data [1]);
			lightSynths.AddFirst (new LightSynth (sus, excitement, amp));
		} else if (packet.Address == "/scsynth/ambientSynth") {
			float sus = System.Convert.ToSingle (packet.Data [0]);
			float amp = System.Convert.ToSingle (packet.Data [1]);
			ambientSynths.AddFirst (new AmbientSynth (sus, excitement, amp));
		} else if (packet.Address == "/scsynth/bigSynth") {
			float sus = System.Convert.ToSingle (packet.Data [0]);
			float amp = System.Convert.ToSingle (packet.Data [1]);
			bigSynths.AddFirst (new BigSynth (sus, excitement, amp));
		} else if (packet.Address == "/scsynth/kick") {
			cameraScript.shake = true;
			cameraScript.shakeFrames = 0;
//			print ("cam shake is :" + camera.shake);
		} else if (packet.Address == "/evaluate") {
//			evalText.GetComponent<CodeFlash>().textFlash(packet.Data [0].ToString ());
			evalTextString = packet.Data [0].ToString ();
			newEvalText = true;
		} else if (packet.Address == "/solo") {
			var id = System.Convert.ToInt32 (packet.Data [0]);
			var duration = System.Convert.ToSingle (packet.Data [1]);
			print ("****solo received for user: " + id + " for duration: " + duration);
			soloDur = duration;
			newSolo = true;
			soloID = id;
			solo = true;
		} 
		else {
			print ("Unity received OSC with no expected address:  "+packet.Address);
		}
	}

	// Update is called once per frame
	void Update () {

		var emission = dustPS.emission;
		emission.rateOverTimeMultiplier = 40+depth*80;


		float meanExcitement = 0;
		float meanDiversity = 0;
		float meanDepth = 0;
		if (activeAudienceMembers.Count != 0) {

			foreach (KeyValuePair<int, GameObject> entry in activeAudienceMembers) {
				meanExcitement += entry.Value.GetComponent<Audience> ().excitement;
				meanDiversity += entry.Value.GetComponent<Audience> ().diversity;
				meanDepth += entry.Value.GetComponent<Audience> ().depth;

			}

			meanDepth = meanDepth / activeAudienceMembers.Count;
			meanDiversity = meanDiversity / activeAudienceMembers.Count;
			meanExcitement = meanExcitement / activeAudienceMembers.Count;

		}

		if (newSolo) {
			soloTime = Time.time + soloDur;
			newSolo = false;
		}

		if (Time.time > soloTime && solo) {
			solo = false;
			if (activeAudienceMembers.ContainsKey (soloID)) {
				activeAudienceMembers [soloID].GetComponent<Audience> ().solo = false;
			} else {
				print ("WARNING attempted to remove solo from non-existant audience member");
			}
		}


		if (solo) {

			if (activeAudienceMembers.ContainsKey (soloID)) {
				var soloist = activeAudienceMembers [soloID].GetComponent<Audience> ();
				soloist.solo = true;
				excitement = Mathf.Clamp (soloist.excitement, 0, 1);
				depth = Mathf.Clamp (soloist.depth, 0, 1);
				diversity = Mathf.Clamp (soloist.diversity, 0, 1);
			} else{
				print ("WARNING attempted to give solo to non-existant audience member  -  Layer 3's set to normal values.");
				excitement = Mathf.Clamp ((1 - coherence) * lcExcitement + coherence * meanExcitement, 0, 1);
				depth = Mathf.Clamp ((1 - coherence) * lcDepth + coherence * meanDepth, 0, 1);
				diversity = Mathf.Clamp ((1 - coherence) * lcDiversity + coherence * meanDiversity, 0, 1);
			}

		} else {
			excitement = Mathf.Clamp ((1 - coherence) * lcExcitement + coherence * meanExcitement, 0, 1);
			depth = Mathf.Clamp ((1 - coherence) * lcDepth + coherence * meanDepth, 0, 1);
			diversity = Mathf.Clamp ((1 - coherence) * lcDiversity + coherence * meanDiversity, 0, 1);
		}


//		OSCMessage oscPack = new OSCMessage("/l3",excitement,depth,diversity);
//		float[] vals = new float[3]{excitement,depth,diversity};
//		// create a byte array and copy the floats into it...
//		var byteArray = new byte[12];
//		System.Buffer.BlockCopy(vals, 0, byteArray, 0, byteArray.Length);

		try{
			scOSC.Send(new OSCMessage("/unityL3/excitement",excitement));
			scOSC.Send(new OSCMessage("/unityL3/diversity",diversity));
			scOSC.Send(new OSCMessage("/unityL3/depth",depth));
		} catch (System.Exception e){
			print ("Warning, SC refusing OSC messages");
		}

		if (newEvalText) {
			evalText.GetComponent<CodeFlash> ().textFlash (evalTextString);
			newEvalText = false;
		}

		camera.GetComponent<CameraScript>().excitement = excitement;
		drawLightSynths ();
		drawAmbientSynths ();
		drawBigSynths ();
		setRaveLights ();
		drawText();
		drawAudienceMembers ();
		removeAudienceMembers ();
		//@fio what the real problem is here...
		try {
			updateAudienceObjects ();
		} catch (System.Exception e){
			print ("error updating audience..." + e);
		}
		nudgeAudience ();



		if (beatCount <= 0) {
			beat = 0;
		} else {
			beatCount = beatCount - 1;	
		}

		excitementText.text = ("Excitement:  " + string.Format ("{0:N2}", excitement));
		diversityText.text =  ("Diversity:   " + string.Format ("{0:N2}", diversity));
		depthText.text =      ("Depth:       " + string.Format ("{0:N2}", depth));

	}// End of update //////////////////////////////////////////////////////////////////////////////////////////////////////////
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


	void updateAudienceObjects(){
		if (updateAudienceMembers.Count != 0) {
			for (LinkedListNode<Audience> b = updateAudienceMembers.First; b != null; b = b.Next) {
				activeAudienceMembers [b.Value.id].GetComponent<Audience>().setTo (b.Value);
			}
			updateAudienceMembers.Clear ();
		}
	}

	void nudgeAudience(){
		if (nudgeAudienceMembers.Count != 0) {
			foreach (KeyValuePair<int,Vector3> member in nudgeAudienceMembers) {
				if (activeAudienceMembers.ContainsKey(member.Key)){
//					activeAudienceMembers [member.Key].GetComponent<Rigidbody> ().AddForce (member.Value);
					activeAudienceMembers [member.Key].GetComponent<Audience> ().nudge(member.Value);

				}
			}
			nudgeAudienceMembers.Clear ();
		}
	}
				
	void removeAudienceMembers(){
		if (deleteAudienceMembers.Count != 0) {
			for (LinkedListNode<int> b = deleteAudienceMembers.First; b != null; b = b.Next) {
				Destroy(activeAudienceMembers[b.Value]);
				activeAudienceMembers.Remove (b.Value);
			}
			deleteAudienceMembers.Clear ();
		}
	}

	void setRaveLights(){
		ParticleSystem.EmissionModule gPS = greenRaveLight.GetComponent<ParticleSystem> ().emission;
		ParticleSystem.EmissionModule tPS = turqouiseRaveLight.GetComponent<ParticleSystem> ().emission;
		ParticleSystem.EmissionModule pPS = purpleRaveLight.GetComponent<ParticleSystem> ().emission;
		float[] weights = getWeights ();
		tPS.rateOverTime = 180 * weights [0] * excitement;
		gPS.rateOverTime = 180 * weights [2] * excitement;
		pPS.rateOverTime = 1840 * weights [1] * excitement;
	}

	void drawAudienceMembers(){
		if (newAudienceMembers.Count != 0) {
			for (LinkedListNode<Audience> b = newAudienceMembers.First; b != null; b = b.Next) {
				Vector3 pos = new Vector3 (Random.Range(-10,10), Random.Range(3,10), Random.Range(-10,10));
				GameObject obj = Instantiate (audienceObj, pos, Quaternion.identity) as GameObject;
				Audience aud = obj.GetComponent<Audience> ();
				aud.oscController = this;
				aud.username = b.Value.username;
				aud.id = b.Value.id;
				activeAudienceMembers.Add(aud.id, obj);
			}
			newAudienceMembers.Clear ();
		}
	}



	void drawText(){
		string wrappedString = "";
		string condensedString = "";
		int n=0;
		int newLines = 0;

		//Get rid of extra newlines
		for (int i = 1; i < updatedText.Length; i += 1) {
			if (updatedText [i] == '\n' && updatedText [i - 1] == '\n') {
			} else {
				condensedString = condensedString + updatedText [i];
			}
		}

		for (int i = 0; i < condensedString.Length-1 && newLines<TextScript.maxLines; i +=1) {
			if (n == TextScript.textWidth) {
				newLines = newLines + 1;
				wrappedString = wrappedString + "\n";
				n = 0;
			}
			if (condensedString [i] == '\n') {
				n = 0;
				newLines = newLines + 1;
			} else {
				n = n + 1;
			}
			wrappedString = wrappedString + condensedString [i];
		}
				
		textMesh.text = wrappedString;
	}





	void drawAmbientSynths(){
		if (ambientSynths.Count != 0) {

			for (LinkedListNode<AmbientSynth> b = ambientSynths.First; b != null; b = b.Next) {
				Vector3 pos = new Vector3 (Random.Range(-30,30), ambSynthY, Random.Range(-10,frontWall.transform.position.z-1));
				GameObject obj = Instantiate (ambientSynthObj, pos, Quaternion.identity) as GameObject;
				AmbientSynth amb = obj.GetComponent<AmbientSynth> ();
				amb.amp = b.Value.amp;
				Color hmm = getWeightedColor ();
				amb.setColor (hmm);
				amb.oscController = this;
				amb.setExpTime (b.Value.expireTime);
				ambSynthY = ambSynthY % 32;
				ambSynthY = ambSynthY + 8;
			}
			ambSynthY = ambSynthY % 512;
			ambientSynths.Clear ();
		}
	}

	void drawLightSynths(){
		if (lightSynths.Count != 0) {
			float generalXVelocity = Random.Range (-1f, 1f);
			float generalYVelocity = Random.Range (-1f, 1f);
			float generalZVelocity = Random.Range (-1f, frontWall.transform.position.z-1);
			float generalPosX = (Random.Range (0, 2) * 2 - 1)*40;
			float generalPosY = Random.Range (10f, 20f);

			//@ 20*depth just 'known' from what I'm using in the front wall script.. should probably be getting this value
			//  straight from the wall's depth
			float generalPosZ = Random.Range (-15, (FrontWall.MAX_DEPTH/8) * depth);
			int i = 0;
			int jx = Random.Range (0, 2) * 2 - 1;
			int jy = Random.Range (0, 2) * 2 - 1;
			int jz = Random.Range (0, 2) * 2 - 1;
			Color col = getWeightedColor ();

			for (LinkedListNode<LightSynth> b = lightSynths.First; b != null; b = b.Next) {

				GameObject obj = (Instantiate (lightSynthObj, new Vector3 (generalPosX + i * 4 * jx, generalPosY + i * jy, generalPosZ + i * 4 * jz), Quaternion.identity)as GameObject);
				LightSynth ls = obj.GetComponent<LightSynth> ();
				ls.oscController = this;

				ls.expireTime = b.Value.expireTime;
				float spread = Random.Range (0, 0.5f);
				ls.setColor (col);
				ls.amp = b.Value.amp;
				ls.setVelocity (new Vector3 (generalXVelocity + spread, generalYVelocity + spread, generalZVelocity + spread) * 8 * excitement);
//				ls.oscClient = new OSCClient (new System.Net.IPAddress (new byte[] { 127, 0, 0, 1 }), 9004);
				ls.oscClient = new OSCClient (scIP, 9004);

				i = i + 1;
			}
			lightSynths.Clear ();
		}
	}

	void drawBigSynths(){

		if (bigSynths.Count != 0) {
			float generalXVelocity = Random.Range (-1f, 1f);
			float generalYVelocity = Random.Range (-1f, 1f);
			float generalZVelocity = Random.Range (-1f, frontWall.transform.position.z-1);
			float generalPosY = Random.Range (-5f, 10f);
			float generalPosX = (Random.Range (0, 2) * 2 - 1)*30;

			//@ 20*depth just 'known' from what I'm using in the front wall script.. should probably be getting this value
			//  straight from the wall's depth
			float generalPosZ = Random.Range (-15, (FrontWall.MAX_DEPTH/8) * depth);
			int i = 0;
			int jx = Random.Range (0, 2) * 2 - 1;
			int jy = Random.Range (0, 2) * 2 - 1;
			int jz = Random.Range (0, 2) * 2 - 1;

			for (LinkedListNode<BigSynth> b = bigSynths.First; b != null; b = b.Next) {
				GameObject obj = (Instantiate (bigSynthObj, new Vector3 (generalPosX + i * 4 * jx, generalPosY + i * jy + 10, generalPosZ + i * 4 * jz), Quaternion.identity)as GameObject);

				BigSynth bs = obj.GetComponent<BigSynth> ();
				Color col = getWeightedColor ();
				bs.setColor (col);
				bs.setCollisionColor (col);
				bs.expireTime = b.Value.expireTime;
				float spread = Random.Range (0, 0.5f);
				bs.oscController = this;
				bs.amp = b.Value.amp;
				bs.transform.localScale = bs.transform.localScale * (Mathf.Log(bs.amp+0.5f,2)+1)*4;
//				bs.setVelocity (new Vector3 (generalXVelocity + spread, generalYVelocity + spread, generalZVelocity + spread) * 8 * excitement);
				bs.setVelocity (new Vector3 (generalPosX * (-1), generalYVelocity + spread, generalZVelocity + spread) * 8 * excitement);
//				bs.oscClient = new OSCClient (new System.Net.IPAddress (new byte[] { 127, 0, 0, 1 }), 9004);
				bs.oscClient = new OSCClient (scIP, 9004);


				i = i + 1;
			}

			bigSynths.Clear ();
			
		}
	}


	private float[] getWeights(){
		float[] weights = new float[3];

		if (diversity == 0) {
			weights = new float[] {1,0,0};
		}
		else if (diversity < 0.1) {
			weights = new float[]{ 0.9f, 0.05f, 0.05f };
		} else if (diversity < 0.2){
			weights = new float[]{ 0.8f, 0.1f, 0.1f };
		} else if (diversity < 0.3){
			weights = new float[]{ 0.7f, 0.2f, 0.1f };
		} else if (diversity < 0.4){
			weights = new float[]{ 0.6f, 0.3f, 0.1f };
		} else if (diversity < 0.5){
			weights = new float[]{ 0.5f, 0.35f, 0.15f };
		} else if (diversity < 0.6){
			weights = new float[]{ 0.5f, 0.3f, 0.2f };
		} else if (diversity < 0.7){
			weights = new float[]{ 0.45f, 0.35f, 0.20f };
		} else if (diversity < 0.8){
			weights = new float[]{ 0.4f, 0.35f, 0.25f};
		} else if (diversity < 0.9){
			weights = new float[]{ 0.35f, 0.35f, 0.3f };
		} else {
			weights = new float[]{0.33333f,0.3333f,0.3333f};
		}
		return weights;
	}

	private int getWeight(){
		int weight;
		if (diversity == 0) {
			weight = 0;
		}
		else if (diversity < 0.1) {
			weight = RandFuncs.Sample (new float[]{ 0.9f, 0.05f, 0.05f});
		} else if (diversity < 0.2){
			weight = RandFuncs.Sample (new float[]{ 0.8f, 0.1f, 0.1f});
		} else if (diversity < 0.3){
			weight = RandFuncs.Sample (new float[]{ 0.7f, 0.2f, 0.1f });
		} else if (diversity < 0.4){
			weight = RandFuncs.Sample (new float[]{ 0.6f, 0.3f, 0.1f });
		} else if (diversity < 0.5){
			weight = RandFuncs.Sample (new float[]{ 0.5f, 0.35f, 0.15f });
		} else if (diversity < 0.6){
			weight = RandFuncs.Sample (new float[]{ 0.5f, 0.3f, 0.2f });
		} else if (diversity < 0.7){
			weight = RandFuncs.Sample (new float[]{ 0.45f, 0.35f, 0.20f });
		} else if (diversity < 0.8){
			weight = RandFuncs.Sample (new float[]{ 0.4f, 0.35f, 0.25f});
		} else if (diversity < 0.9){
			weight = RandFuncs.Sample (new float[]{ 0.35f, 0.35f, 0.3f });
		} else {
			weight = RandFuncs.Sample (new float[]{0.33333f,0.3333f,0.3333f });
		}
		return weight;
	}

	private Color getWeightedColor(){
		return colors [getWeight ()];
	}



}
//		if (synthSpawnTrigger == true) {
//			float generalXVelocity = Random.Range (-1f, 1f);
//			float generalYVelocity = Random.Range (-1f, 1f);
//			float generalZVelocity = Random.Range (-1f, 1f);
//			float generalPos = Random.Range (-5f, 5f);
//
//			print ("!!@@@@@@@" + generalXVelocity);
//			int i = 0;
//			for (LinkedListNode<SynthAvatarScript> b = synthAvatars.First; b != null; b = b.Next) {
//			
//				GameObject obj = (Instantiate (synthObj, new Vector3 (generalPos + i * 4, generalPos + i * 2 + 6, generalPos + i * 4), Quaternion.identity)as GameObject);
////				GameObject obj = (Instantiate (synthObj, new Vector3 (generalPos,generalPos+i*10+6,generalPos), Quaternion.identity)as GameObject);
//				//Initialize values of new synth - @Feels like I should be able to initialize
//				// the instances when osc is recieved, and then instantiate from those instances?
//				obj.GetComponent<SynthAvatarScript> ().expireTime = b.Value.expireTime;
//				obj.GetComponent<SynthAvatarScript> ().excitement = excitement;
////				obj.GetComponent<SynthAvatarScript> ().setVelocity(new Vector3 (Random.Range (-1, 1), Random.Range (-1, 1), Random.Range (-1, 1)) * b.Value.amp*20);
//				float spread = Random.Range (-0.0125f, 0.0125f);
//				spread = 0;
//				obj.GetComponent<SynthAvatarScript> ().setVelocity (new Vector3 (generalXVelocity + spread, generalYVelocity + spread, generalZVelocity + spread) * b.Value.amp * 30);
////				obj.GetComponent<SynthAvatarScript> ().setVelocity(new Vector3 (generalXVelocity,generalYVelocity,generalZVelocity)* b.Value.amp*30);
//
//				obj.GetComponent<SynthAvatarScript> ().oscClient = new OSCClient (new System.Net.IPAddress (new byte[] {
//					127,
//					0,
//					0,
//					1
//				}), 9004);
//
//				i = i + 1;
//			}
//
//			synthAvatars.Clear ();
//			synthSpawnTrigger = false;
//		}


//
//	void LateUpdate(){
//		if (synthSpawnTrigger == true) {
//			//			SynthAvatarScript obj = Instantiate (synthObj, new Vector3 (Random.Range (-5, 5), 1, Random.Range (-5, 5)), Quaternion.identity);
//			//			GameObject obj = (Instantiate(synthObj) as GameObject).GetComponent<SynthAvatarScript>().setExpTime(synthExpTime);
//			//			GameObject obj = Instantiate (synthObj, new Vector3 (Random.Range (-5, 5), 1, Random.Range (-5, 5)), Quaternion.identity);
//			GameObject obj = (Instantiate (synthObj, new Vector3 (Random.Range (-5, 5), 1, Random.Range (-5, 5)), Quaternion.identity)as GameObject);
//			//			obj = (Instantiate (synthObj, new Vector3 (Random.Range (-5, 5), 1, Random.Range (-5, 5)), Quaternion.identity)as GameObject);
//			//			SynthAvatarScript.Create (synthExpTime);
//			//			obj.GetComponent<SynthAvatarScript> ().amp = (synthAmp);
//
//			//			 obj.setExpTime (synthExpTime); 
//			synthSpawnTrigger = false;
//		}
//	
//	}