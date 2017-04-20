Seesaw{
	classvar <>mainBus;
	var <>scale;
	var <>excitement;
	var <>diversity;
	var <>depth;

	*connectSynth{
		~outSynth = Synth(\out,addAction:\addToTail);

	}

	*start{
		|unityIP="127.0.0.1"|
		Seesaw.loadSynths();
		Seesaw.oscDefs(unityIP);
		Seesaw.tdefs();
		Seesaw.startPatterns();
		Seesaw.midiDefs();
	}

	*stopPatterns{
		Pdef(\bass).stop;
		Pdef(\hihat).stop;
		Pdef(\hihat1).stop;
		Pdef(\hihat2).stop;
		Pdef(\hihat3).stop;
		Pdef(\arpeg).stop;
		Pdef(\snare).stop;
		Pdef(\snare2).stop;
		Pdef(\pad).stop;

	}

	*loadSynths{
		Server.default.waitForBoot({
			~mainBus = Bus.audio(Server.default,2).index;
			~sn2 = Buffer.read(Server.default, "/Users/JamieBeverley/Desktop/samples/sn/ST0T0S3.wav");

			Tdef(\loadSynths,{
				2.wait;
				"~/Desktop/Thesis/ThesisFinal/SuperCollider/Synths.scd".loadPaths;
				2.wait;
				~outSynth = Synth(\out,addAction:\addToTail);
				"synths loaded".postln;
			}).play;
		},onFailure:"could not boot sc server".warn;);
	}

	*startPatterns{
		Pdefn(\root,0);
		~scale = Scale.major;
		~degree = 0;
		Pdefn(\scale,Pfunc({~scale}));
		Pdef(\master,Pbind(\root,Pdefn(\root),\scale,Pdefn(\scale),\out,[0,1]));
		TempoClock.tempo= 120/60;
		if (~excitement == nil,{~excitement =0});
		if (~diversity == nil,{~diversity =0});
		if (~depth == nil,{~depth =0});

		"~/Desktop/Thesis/ThesisFinal/SuperCollider//Patterns.scd".loadPaths;
	}

	*oscDefs{
		|unityIP="127.0.0.1"|
		~unity = NetAddr(unityIP, 9003);
		~node = NetAddr("127.0.0.1", 9000);
		Audience.nodeServer = ~node;

		if ((~audienceMembers==nil),{
			~audienceMembers = Dictionary.new();
		});



		OSCdef(\light,{
			|msg|
			// msg.postln;
			~unity.sendMsg(msg[0],msg[3],msg[4].clip(0,1));
		},"/scsynth/lightSynth");
		OSCdef(\ambient,{
			|msg|
			// msg.postln;
			~unity.sendMsg(msg[0],msg[3],msg[4].clip(0,1));
		},"/scsynth/ambientSynth");

		OSCdef(\big,{
			|msg|
			~unity.sendMsg(msg[0],msg[3],msg[4].clip(0,1));
		},"/scsynth/bigSynth");

		OSCdef(\kick,{
			|msg|
			~unity.sendMsg(msg[0]);
		},"/scsynth/kick");


		OSCdef(\giveSolo, {
			|msg|
			Seesaw.giveSolo(~audienceMembers[msg[1]].username,8);
		},"/solo",recvPort:9001);

		OSCdef(\relayNewAudience,{
			|msg|
			var aud = Audience.new(msg[1],msg[2],0.5,diversity:0.5,excitement:0.5,depth:0.5);
			if (~audienceMembers==nil,{~audienceMembers=Dictionary();});
			~unity.sendMsg(msg[0],msg[1],msg[2]);
			~audienceMembers.add(aud.id -> aud);
		},"newAudienceMember",recvPort:9001);

		OSCdef(\relayRemoveAudience,{
			|msg|
			~unity.sendMsg(msg[0],msg[1],msg[2]);
			~audienceMembers.removeAt(msg[1]);
		},"removeAudienceMember",recvPort:9001);

		OSCdef(\relayNudge,{
			|msg|
			// address, audienceID, x ,y ,z
			~unity.sendMsg(msg[0],msg[1],msg[2],msg[3],msg[4]);
		},"/nudge",recvPort:9001);

		OSCdef(\audienceUpdate,{
			|msg|
			var id = msg[1];
			var username = msg[2];
			var scale = msg[3];
			var diversity = msg[4];
			var excitement = msg[5];
			var depth = msg[6];
			if (~audienceMembers.at(id)!=nil,{
				~audienceMembers[id] = Audience(id,username,scale,diversity,excitement,depth);
				~unity.sendMsg("/updateAudience",id,username,scale, diversity, excitement, depth);
			},{
				"No audience member at update key - You may just want to restart the node server".warn;
			});
		},"updateAudience",recvPort:9001);


		OSCdef(\unityL3Excitement,{
			|msg|
			~excitement=msg[1];
		},"/unityL3/excitement",recvPort:9004);
		OSCdef(\unityL3Diversity,{
			|msg|
			~diversity=msg[1];
		},"/unityL3/diversity",recvPort:9004);

		OSCdef(\unityL3Depth,{
			|msg|
			~depth=msg[1];
			~outSynth.set(\depth,~depth);
		},"/unityL3/depth",recvPort:9004);

		OSCdef(\unityShimmer,{
			|msg|
			// 1 ID, 2- freq, 3 sus
			~audienceMembers[msg[1]].send("/shimmer",msg[2],msg[3]);
		},"/shimmer",recvPort:9004);

		OSCdef(\coherence,{
			|msg|
			~coherence = msg[1].clip(0,1);
			~unity.sendMsg("/coherence",~coherence);
		},path:"/node/coherence",recvPort:9001);

		OSCdef(\motion,{
			|msg|
			~motion = msg[1].clip(0,1);
		},path:"/node/motion",recvPort:9001)

	}

	*tdefs{

		if (~performerExcitement == nil,{~performerExcitement =0});
		if (~performerDiversity == nil,{~performerDiversity =0});
		if (~performerDepth == nil,{~performerDepth =0});
		if (~octave == nil, {~octave=4});
		if (~scale == nil, {~scale = Scale.major});

		Tdef(\sendPitchData,{
			inf.do{
				~node.sendMsg("/updateFreq",(12*(~octave)+~scale.degrees.choose).midicps.asString);
				0.125.wait;
			}
		}).play;

		Tdef(\sendL3,{
			inf.do{
				~unity.sendMsg("/l3",~performerExcitement,~performerDiversity, ~performerDepth);
				// ~unity.sendMsg("/activity",~activity);
				0.125.wait;
			}
		}).play;

		Tdef(\sendCode, {
			inf.do{
				~unity.sendMsg("/text",Document.current.string);
				(0.25).wait;
			}
		}).play;
		/*		//@ this should be changed to a Pdef/something that updates with tempo changes
		Tdef(\sendBeat,{
		inf.do{
		~unity.sendMsg("/beat", 1);
		"beat".postln;
		TempoClock.tempo.wait;
		}
		}).play;*/
	}

	*stopTdefs{
		Tdef(\sendCode).stop;
		Tdef(\sendL3).stop;
	}

	*midiDefs{
		MIDIClient.init;
		MIDIIn.connectAll;
		MIDIdef(\lpd8Vol,{
			|val,nm,chan,src|
			if (nm==1,{
				~vol = val/127;
				~outSynth.set(\vol,~vol);

			});
			if (nm==2,{
				~performerExcitement = val/127;
			});
			if (nm==3,{
				~performerDiversity = val/127;
			});
			if (nm==4,{
				~performerDepth = (val/127).clip(0,1);
				~outSynth.set(\depth,~depth);
				// ~outSynth.set(\lpf,~depth.linexp(0,1,2000,800));
				// ~outSynth.set(\reverbDepth,~depth);
			});
			(val/127).postln;
		},msgType:\control);
	}

	*giveSolo{
		|username, duration=16|
		username.postln;
		for(0,~audienceMembers.size-1,{
			|i|
			var user = ~audienceMembers[~audienceMembers.keys.asArray[i]];
			var sentSolo = false;

			user.username.postln;
			if ((user.username.asString.toLower==username.toLower),{
				user.send("/solo",duration);
				~unity.sendMsg("/solo",user.id,duration);
				sentSolo = true;
			});

			if(sentSolo.not,{
				"User not found, username does not exist.".warn;
			});
		});
	}

	//@make this better
	*connect{
		|audienceID, audienceParam, performerParam|
		var val;
		if (audienceParam.toLower == "excitement",{
			val = ~audienceMembers[audienceID].excitement;
			Tdef(\excitementConnect,{
				inf.do{
					~performerExcitement=val;
					0.05.wait;
				}
			});
		});

		if (audienceParam.toLower == "diversity",{
			val = ~audienceMembers[audienceID].diversity;
			Tdef(\diversityConnect,{
				inf.do{
					~performerDiversity=val;
					0.05.wait;
				}
			});
		});

		if (audienceParam.toLower == "depth",{
			val = ~audienceMembers[audienceID].depth;
			Tdef(\depthConnect,{
				inf.do{
					~performerDepth=val;
					0.05.wait;
				}
			});
		});
	}

	*disconnect{
		|param|

		if (param.toLower == "excitement",{
			Tdef(\excitementConnect).stop;
		});

		if (param.toLower == "diversity",{
			Tdef(\diversityConnect).stop;
		});

		if (param.toLower == "depth",{
			Tdef(\depthConnect).stop;
		});

	}
}


Audience : Object{
	classvar <>nodeServer;
	var <>id;
	var <>username;
	var <>scale;
	var <>diversity;
	var <>excitement;
	var <>depth;

	*new{
		|id, uName, scale, diversity, excitement, depth|
		^super.new.init(id, uName, scale, diversity, excitement, depth);
	}

	*playOnAllX{
		|xCondition, freq, dur|
		~audienceMembers.do({
			|x|
			if (xCondition.(x),{
				x.play(freq,dur);
			});
		});
	}

	*sendToAllX{
		|xCondition, address, arg1=0, arg2=0, arg3=0, arg4=0|
		~audienceMembers.do({
			|x|
			if (xCondition.(x),{
				x.send(address,arg1,arg2,arg3,arg4);
			});
		});
	}

	*showAllX {
		|xCondition|
		~audienceMembers.do({
			|x|
			if (xCondition.(x),{
				("uName:  "+x.username+"   ID:  "+x.id).postln;
			});
		});
	}

	*showMax {
		|param|
		var maxExcitement =0;
		var maxDiversity = 0;
		var maxDepth = 0;
		var maxExcitementID;
		var maxDiversityID;
		var maxDepthID;

		~audienceMembers.do({
			|x|
			if (x.excitement>maxExcitement,{
				maxExcitementID = x.id;
				maxExcitement = x.excitement;
			});

			if (x.diversity>maxDiversity,{
				maxDiversityID = x.id;
				maxDiversity = x.diversity;
			});

			if (x.depth>maxDepth,{
				maxDepthID = x.id;
				maxDepth = x.depth;
			});
		});

		if (param.toLower == "excitement",{
			("uName:  "+~audienceMembers[maxExcitementID].username).postln;
		});

		if (param.toLower == "diversity",{
			("uName:  "+~audienceMembers[maxDiversityID].username).postln;
		});

		if (param.toLower == "depth",{
			("uName:  "+~audienceMembers[maxDepthID].username).postln;
		});
	}

	*showMin{
		|param|
		Seesaw.getMin(param);
	}

	*getMax {
		|param|
		var maxExcitement =0;
		var maxDiversity = 0;
		var maxDepth = 0;
		var maxExcitementID;
		var maxDiversityID;
		var maxDepthID;

		~audienceMembers.do({
			|x|
			if (x.excitement>maxExcitement,{
				maxExcitementID = x.id;
				maxExcitement = x.excitement;
			});

			if (x.diversity>maxDiversity,{
				maxDiversityID = x.id;
				maxDiversity = x.diversity;
			});

			if (x.depth>maxDepth,{
				maxDepthID = x.id;
				maxDepth = x.depth;
			});
		});

		if (param.toLower == "excitement",{
			("uName:  "+~audienceMembers[maxExcitementID].username).postln;

			^~audienceMembers[maxExcitementID];
		});

		if (param.toLower == "diversity",{
			("uName:  "+~audienceMembers[maxDiversityID].username).postln;
			^~audienceMembers[maxDiversityID];
		});

		if (param.toLower == "depth",{
			("uName:  "+~audienceMembers[maxDepthID].username).postln;
			^~audienceMembers[maxDepthID];
		});
	}

	*getMin {
		|param|
		var minExcitement =0;
		var minDiversity = 0;
		var minDepth = 0;
		var minExcitementID;
		var minDiversityID;
		var minDepthID;

		~audienceMembers.do({
			|x|
			if (x.excitement<minExcitement,{
				minExcitementID = x.id;
				minExcitement = x.excitement;
			});

			if (x.diversity<minDiversity,{
				minDiversityID = x.id;
				minDiversity = x.diversity;
			});

			if (x.depth<minDepth,{
				minDepthID = x.id;
				minDepth = x.depth;
			});
		});

		if (param.toLower == "excitement",{
			("uName:  "+~audienceMembers[minExcitementID].username).postln;

			^~audienceMembers[minExcitementID];
		});

		if (param.toLower == "diversity",{
			("uName:  "+~audienceMembers[minDiversityID].username).postln;
			^~audienceMembers[minDiversityID];
		});

		if (param.toLower == "depth",{
			("uName:  "+~audienceMembers[minDepthID].username).postln;
			^~audienceMembers[minDepthID];
		});
	}

	play{
		|freq,dur|

		if ((Audience.nodeServer!=nil),{
			Audience.nodeServer.sendMsg("/shimmer",this.id, freq,dur);
		});
	}

	send{
		|address,arg1=0,arg2=0,arg3=0,arg4=0|
		// (Audience.nodeServer).postln;
		if((Audience.nodeServer!=nil),{
			Audience.nodeServer.sendMsg(address,this.id,arg1,arg2,arg3,arg4);
		},{
			"Node server address not set".warn;
		});
	}

	init{
		|id, uName, scale, diversity, excitement, depth|
		this.id= id.asInt;
		this.username = uName.asString;
		this.scale = scale;
		this.diversity = diversity.asFloat;
		this.excitement = excitement.asFloat;
		this.depth = depth.asFloat;
	}

}