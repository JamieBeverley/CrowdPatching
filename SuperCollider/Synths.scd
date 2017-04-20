(
SynthDef(\out,{
	|depth=0, vol=1|
	var reverbDepth,lpf;
	var audio = In.ar(~mainBus,2);

	lpf = depth.linexp(0,1,20000,1500);

	// reverbDepth= depth.linexp(0,1,0,0.7);
	reverbDepth = depth*0.5;
	audio = FreeVerb.ar(audio,mix:reverbDepth,room:0.9,damp:0.9);
	audio = RLPF.ar(audio,lpf,0.4);
	audio = audio*(vol.clip(0,1));
	audio = Compander.ar(audio,audio,thresh:-10.dbamp,slopeAbove:1/10);
	Out.ar(0,audio);
}).add;
);


(SynthDef(\kick, {
	|freq=32, amp=0.1, out=0, gate=1,at=0.002,dur|
	var audio,env;
	amp = amp.clip(0,1);
	freq = freq.clip(0,130);
	audio = SinOsc.ar(freq:freq, mul:amp*1.5);
	audio = LPF.ar(audio,freq:freq*2*XLine.ar(4,1,at.clip(0.0001,1)/2)/2,mul:6.dbamp);
	audio = audio +LPF.ar(PinkNoise.ar(amp*4)*XLine.ar(2,0,at/2),freq*4);
	env = EnvGen.ar(Env.adsr(at,0.3,sustainLevel:0.1,releaseTime:0.25),gate:gate,doneAction:2);
	// audio = audio*EnvGen.ar(Env.adsr(at,0.1,sustainLevel:0.4,releaseTime:0.25),gate:gate,doneAction:0);
	// audio = audio*EnvGen.ar(Env.perc(at,releaseTime:1.5,curve:-8));
	SendReply.kr(Line.kr(-1,1,at),cmdName:"/scsynth/kick",values:[amp]);
	audio = audio*env;

	Out.ar(~mainBus,Pan2.ar(audio,0));
}).add;
(instrument:\kick,\midinote:30,out:[0,1],\db:-20).play;
);

(
SynthDef(\bass, {
	|freq=130, amp=0.1, out=0, gate=1,at=0.01,dt=0.03,sawMix=10,sustain,modMul=4|
	var audio,env,sawEnv,mod, modEnv;
	amp = amp.clip(0,1);
	modEnv = EnvGen.ar(Env.linen(at,0,sustain,0.5));//(at,dt*40,0.1,releaseTime:1),gate:gate,doneAction:2);
	mod = Mix.ar(SinOsc.ar(freq:freq/2, mul:freq*modMul)*modEnv);
	audio = Mix.ar(SinOsc.ar(freq:freq+mod, mul:amp/3)+SinOsc.ar(freq:freq*1.5+mod, mul:amp/8));
	env = EnvGen.ar(Env.adsr(at,dt,sustainLevel:0.2,releaseTime:0.25),gate:gate,doneAction:2);
	audio = audio*env;

	SendReply.kr(Line.kr(-1,1,at),cmdName:"/scsynth/bigSynth",values:[sustain,amp]);

	Out.ar(~mainBus,Pan2.ar(audio,0));
}).add;

(instrument:\bass,\midinote:[48].choose,db:-120,out:[0,1],at:[0.1].choose).play;
);

(
SynthDef(\hihat,
	{
		arg out,amp=0.1,dur,rt=0.25,hpf=4000,at=0.001;
		var audio, env;
		amp = amp.clip(0,1);
		audio = HPF.ar(PinkNoise.ar(amp/1),hpf);
		env = EnvGen.ar(Env.perc(attackTime:at,releaseTime:rt,curve:-8),doneAction:2);
		audio = audio*env;
		Out.ar(~mainBus,Pan2.ar(audio,0));
	}
).add;
(\instrument:\hihat,\db:-120).play;
);


(
SynthDef(\pad,
	{
	|freq=440, amp=0.1, out=0, at=0.5, rt=2, lpf, sustain,gate=1,lfoFreq=0.25,resonance=0.15, panScatter=0|
		var audio,env, lpfEnv,lfo;
		amp = amp.clip(0,1);
		resonance = resonance.clip(0,1)/2;
		audio = Mix.ar(Saw.ar([freq,freq*1.02,freq*0.98]++([freq,freq*1.02,freq*0.98]*1.5),mul:amp/6));
		lpfEnv = EnvGen.ar(Env.linen(attackTime:at,sustainTime:sustain,releaseTime:rt),doneAction:0);
		lfo = SinOsc.ar(lfoFreq,mul:0.5,add:1);
		audio = RLPF.ar(audio, freq*0.5*(lpfEnv+0.51)*lfo, resonance.linexp(0, 1, 1, 0.001));
		audio = Compander.ar(audio,audio,0.dbamp,1,slopeAbove:1/20,clampTime:0.001,relaxTime:0.001);
		env = EnvGen.ar(Env.asr(attackTime:at,releaseTime:rt),gate:gate,doneAction:2);
		audio = audio*env;
		audio = Pan2.ar(audio,panScatter*(lfo-1));

		SendReply.kr(Line.kr(-1,1,at),cmdName:"/scsynth/ambientSynth",values:[sustain,amp]);

		Out.ar(~mainBus,Pan2.ar(audio,0));
	}
).add;
(\instrument:\pad,out:0,\midinote:[60,64,67,72].choose,dur:1,at:0.001,\db:-120).play;
);


(
SynthDef(\arpeg,
	{
	|freq=440, amp=0.1, out=0, at=0.5, rt=2, lpf, sustain,gate=1,lfoFreq=0.25,resonance=0.15,arpegFreq=0.0625,panFreq=0.0625,panScatter=0.125|
		var audio,env, arpegEnv,mod, lpfLfO,panEnv;
		amp = amp.clip(0,1);
		arpegEnv = EnvGen.ar(Env.circle([0,0,7,7,12,12,-5,-5,12,12,19,19].midiratio,[arpegFreq,0.01]),doneAction:0);
		panEnv = EnvGen.kr(Env.circle([-1,-1,1,1],[panFreq,0.01]),doneAction:0);
		mod = SinOsc.ar(freq*4,mul:freq);
		audio = SinOsc.ar(freq*arpegEnv+mod,mul:amp/2);
		lpfLfO = SinOsc.ar(0.3,mul:0.25,add:1);
		audio = RLPF.ar(audio,freq*lpfLfO,0.3);
		audio = Compander.ar(audio,audio,0.dbamp,1,slopeAbove:1/20,clampTime:0.001,relaxTime:0.001);
		env = EnvGen.ar(Env.asr(attackTime:at,releaseTime:rt),gate:gate,doneAction:2);
		audio = audio*env;
		audio = Pan2.ar(audio,panEnv*panScatter);
		SendReply.kr(Line.kr(-1,1,at),cmdName:"/scsynth/lightSynth",values:[sustain,amp]);

		Out.ar(~mainBus,Pan2.ar(audio,0));
	}
).add;
(\instrument:\arpeg,out:[0],\midinote:[67].choose,dur:8,at:2,panScatter:0,\db:-120).play;
);



(
SynthDef(\snare,
	{
		|freq=440, amp=0.1, out=0, rate=1,at=0.001, rt=2,sustain,lpf=10000|

		var audio,env;
		lpf = lpf.clip(10,10000);
		amp = amp.clip(0,1);

		audio = amp*PlayBuf.ar(numChannels:~sn2.numChannels,bufnum:~sn2.bufnum,rate:rate,trigger:1, startPos: 0, doneAction:2);

		audio = RLPF.ar(audio,lpf,0.90);
		audio = Compander.ar(audio,audio,0.dbamp,1,slopeAbove:1/20,clampTime:0.001,relaxTime:0.001);

		Out.ar(~mainBus,Pan2.ar(audio,0));
	}
).add;
(\instrument:\snare,\db:-210).play
);