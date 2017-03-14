Jam.loadSynths

// MIDI ____________________________________________________
(
MIDIClient.init;
MIDIIn.connectAll;
MIDIdef(\lpd8Vol,{
	|val,nm,chan,src|
	if (nm==1,{
		~vol = val.linexp(0,127,0.0001,1).ampdb;
	});
	if (nm==2,{
		~excitement = val/127;
	});
	if (nm==3,{
		~diversity = val/127;
	});
	if (nm==4,{
		~depth = val/127;
		~depth.postln;
	});
},msgType:\control);

MIDIdef(\lpd8,{
	|val,nm,chan,src|
	~unity.sendMsg("/newAudienceMember", ~ids, "hmmmm");
	~ids=~ids+1;
	// (instrument:\bigSynth,\midinote:nm,dur:10,db:~vol).play;
	// (instrument:\light,\midinote:nm,dur:4,db:~vol).play;

},msgType:\noteOn);
)
~ids
~ids=0;
Char.n
// Synths __________________________________________________
(
SynthDef(\bigSynth,{
	|freq, amp, sustain=1, out,gate=1,at=0.01|

	var audio = SinOsc.ar(freq,mul:amp);
	var audio1 = Saw.ar(freq,mul:amp);
	var audio2 = Impulse.ar(freq,mul:amp);
	var env = EnvGen.ar(Env.adsr(at,0.3,0.5,1),gate,doneAction:2);
	audio = audio+audio1*0.35+audio2*0.6;
	audio = audio*env;
	audio = LPF.ar(audio,700);
	// audio = audio*Saw.ar(TempoClock.tempo*8,1);

	SendReply.kr(Line.kr(1,0,at),cmdName:"/scsynth/bigSynth",values:[sustain,amp]);

	Out.ar(out,audio);

}).add;


SynthDef(\light,
	{
		arg sustain,amp= -20.dbamp,at=0.01,dur,curve= -8, freq = 800,modPartial=16,delayTime=0.25,out=0;


		var audio = SinOsc.ar(freq+SinOsc.ar(freq:modPartial*freq,mul:modPartial*freq),mul:amp);

		var env = EnvGen.ar(Env.perc(attackTime:at,releaseTime:1,curve:curve),doneAction:0);
		var env1 = EnvGen.ar(Env.pairs([[0,1],[3,1]],\lin),doneAction:2);
		audio = audio*env;

		audio = Decay.ar(audio,decayTime: 0.2,mul:0.1);

		audio = audio+CombC.ar(audio,maxdelaytime: delayTime+0.1,delaytime: delayTime,decaytime:3);
		SendReply.kr(Line.kr(1,0,at),cmdName:"/scsynth/lightSynth",values:[sustain,amp]);

//		audio = audio*env1;

		Out.ar(out, audio);
	}
).add;
SynthDef(\ambient,
	{
		arg freq, amp = 0.1,gate=1,dur,len=1,out=0,sustain,at=0.5,rt=4;
		//var env = EnvGen.ar(Env.linen(attackTime:0.2*dur,sustainTime:0.4*dur, releaseTime:0.4*dur),doneAction:2);
		var env = EnvGen.ar(Env.linen(attackTime:at,sustainTime:sustain, releaseTime:rt),gate:gate,doneAction:2);

		var audio = PinkNoise.ar(freq,mul:amp);

		audio = BPF.ar(audio,freq:freq,rq:0.001)*16;

		audio=audio*env;
	//	audio1=Clip.ar(audio1,-100.dbamp,30.dbamp);
		SendReply.kr(Line.kr(1,-1,at),cmdName:"/scsynth/ambientSynth",values:[sustain+rt,amp]);

		Out.ar(0 ,[audio,audio]);
	}
).add;

)

/// OSC defs ________________________________________________
(
~unity = NetAddr("127.0.0.1", 9003);

OSCdef(\listenToUnity,{
	|msg|
	"asdf".postln;
	msg.postln;
},"/test",recvPort:9004);

OSCdef(\light,{
	|msg|
	// msg.postln;
	~unity.sendMsg(msg[0],msg[3],msg[4].clip(0,1));
	msg.postln;
},"/scsynth/lightSynth");
OSCdef(\ambient,{
	|msg|
	// msg.postln;
	~unity.sendMsg(msg[0],msg[3],msg[4].clip(0,1));
	msg.postln;
},"/scsynth/ambientSynth");

OSCdef(\big,{
	|msg|
	msg.postln;
	~unity.sendMsg(msg[0],msg[3],msg[4].clip(0,1));
},"/scsynth/bigSynth");



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

OSCdef(\audienceUpdate,{
	|msg|
	var id = msg[1];
	var username = msg[2];
	var scale = msg[3];
	var diversity = msg[4];
	var excitement = msg[5];
	var depth = msg[6];
	msg.postln;
	if (~audienceMembers.at(id)!=nil,{
		~audienceMembers[id] = Audience(id,username,scale,diversity,excitement,depth);
		~unity.sendMsg("/updateAudience",msg[1],msg[2],msg[3],msg[4],msg[5],msg[6]);
	},{
		"No audience member at update key".warn;
	});
},"updateAudience",recvPort:9001);


)
~audienceMembers[1].username
~d = ("key"->1)
~d.class
~d.at(2)
~d.add("key"->4);
/// Tdefs ________________________________________________________
(
// ** Note: if these aren't set before sending them to unity,
//          unity's osc gets 'frozen'

~excitement = 0.01;
~depth = 0.01;
~diversity = 0.01;
Tdef(\sendL3,{
	inf.do{
		~unity.sendMsg("/l3",~excitement,~diversity, ~depth);
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
//@ this should be changed to a Pdef/something that updates with tempo changes
Tdef(\sendBeat,{
	inf.do{
		~unity.sendMsg("/beat", 1);
		"beat".postln;
		TempoClock.tempo.wait;
	}
}).play;


)

~audienceMembers
