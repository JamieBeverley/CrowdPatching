//http://www.freesound.org/people/YleArkisto/sounds/316717/ - crowd sound
// in this file:
// 1. connect to web socket server
// 2. send input from html page to ws server
// 3. listen to input from wss and distribute it to html page

try{
var	ws = new WebSocket("ws://"+location.hostname+":"+location.port, 'echo-protocol');
} catch (e){
	console.log("no WebSocket connection")
}
var motionArray = [0,0,0,0,0];
var xi=yi=zi=0;
var delta =0;
//@perhaps just use the audio context clock once this is cleaned up?
var date = new Date();

var isActive = false;

//Scale/Weighting
var scale = 0.5;
var diversity= 0.5;
var excitement = 0.5;
var depth = 0.5;
var globalFreq = 440;

//Web Audio Stuff:
var ac;
var modPartial=1;

var attack = 0.1;
var release =0.5
var freq =0.5
var crowdBufferNode;
var crowdGainVal=0;
var crowdGain;
var crowdPitch = 110; 
var melodyGain;
var osc1, osc2, osc3;


function printInfo(){
	console.log("diversity"+diversity)
	console.log("excitement"+diversity)
	console.log("depth"+diversity)
	console.log("attack"+attack)
}

function start(){
	initWebAudio();
	document.getElementById("mainBody").style.visibility = "visible";
	document.getElementById("startButton").style.visibility = "hidden";
	shimmerAudio(440,4,1)
	isActive = true;
 }


function initWebAudio(){
	window.AudioContext = window.AudioContext || window.webkitAudioContext;
	ac = new AudioContext();
	console.log("Web Audio Initialized");
}

function updateScaleData(){
	var msg = {
		type:"updateWeights",
		scale:scale,
		diversity:diversity,
		excitement:excitement,
		depth: depth
	}		
	ws.send(JSON.stringify(msg))
}

function giveSolo(){
	var msg={
				type: 'solo'
			}
	ws.send(JSON.stringify(msg))
}

function changeUsername(){
	var uName = document.getElementById('username').value;
	msg = {
		type:"changeUsername",
		username: uName
	}
	ws.send(JSON.stringify(msg))
}

function testWebAudio(){
	var mod = ac.createOscillator()
	mod.frequency.value = 440;
	mod.type = 'sine'	
	mod.start();
	var modGain = ac.createGain()
	modGain.gain.value = 440;
	mod.connect(modGain)
	var car = ac.createOscillator()
	car.frequency.value = 440;
	modGain.connect(car.frequency);
	car.type = 'sine'
	var melodyGain = ac.createGain();
	car.connect(melodyGain)
	melodyGain.gain.setValueAtTime(0,ac.currentTime)
	melodyGain.gain.linearRampToValueAtTime(0.1,ac.currentTime+0.4)
	melodyGain.gain.linearRampToValueAtTime(0,ac.currentTime+2)
	melodyGain.connect(ac.destination)
	car.start()
}


function shimmerAudio(freq,sustain,rt){
	freq = Math.max(Math.min(freq,880*4),10)
	sustain = Math.max(0.1)
	// var harmonics = [1,2,1.5,3,4,2,5,6,7,8,9,8,5,4,7,3].reverse().slice(Math.round((1-diversity)*15))
	var div = diversity;
	var harmonics = [1,Math.max(Math.round(div*2),1),Math.max(Math.round(div*3),1),Math.max(Math.round(div*4),1),Math.max(Math.round(div*5),1),Math.max(Math.round(div*6),1),Math.max(Math.round(div*7),1),Math.max(Math.round(div*8),1)] //.reverse().slice((1-diversity)*8)
	
	var mod = ac.createOscillator()
	mod.frequency.value = freq*2*Math.max((1-depth),0.05);
	mod.type = 'sine'	
	mod.start();
	var modGain = ac.createGain()
	modGain.gain.value = freq*4;
	mod.connect(modGain)
	
	var car = ac.createOscillator()
	car.frequency.value = freq/Math.max(depth,0.1);
	modGain.connect(car.frequency);
	car.type = 'sine'

	var i =0
	var c = 0;
	while (i<sustain+rt){
		// var freqJump = harmonics[Math.floor(Math.random()*harmonics.length)];
		freqJump = harmonics[c%harmonics.length]
		car.frequency.setValueAtTime(freq*freqJump,ac.currentTime+i)
		// car.frequency.linearRampToValueAtTime(freq*freqJump,ac.currentTime+i)
		i=i+(0.0125-0.25)*excitement+0.25;
		c=c+1;
	}


	var melodyGain = ac.createGain();
	car.connect(melodyGain)
	melodyGain.gain.setValueAtTime(0,ac.currentTime)
	melodyGain.gain.linearRampToValueAtTime(0.1+excitement*0.1,ac.currentTime+0.4)
	melodyGain.gain.linearRampToValueAtTime(0.1+excitement*0.1,ac.currentTime+0.4+sustain)
	melodyGain.gain.linearRampToValueAtTime(0  ,ac.currentTime+0.4+sustain+rt)

	var filter = ac.createBiquadFilter()
	filter.type = 'lowpass'
	filter.frequency.value = freq;
	filter.Q.value = 3;

	melodyGain.connect(filter)
	filter.connect(ac.destination)
	car.start();
	car.stop(ac.currentTime+sustain+rt+1)

	// setTimeout(function(){
	// 	// delete(car)
	// 	// delete(melodyGain)
	// 	// delete(modGain)
	// 	// delete(mod)
	// },sustain*1000+rt*1000+1000)
}


function nudge(x,y,z){
	var msg = {
		type: 'nudge',
		value: [x,y,z],
	}
	try{
		ws.send(JSON.stringify(msg))
	}
	catch(e){
		console.log("Could not send nudge msg.");
	}
}

function updateL3(){
		diversity=document.getElementById('diversity').value;
		excitement=document.getElementById('excitement').value;
		depth=document.getElementById('depth').value;
}

ws.addEventListener('message', function(message){
	var msg = JSON.parse(message.data)
	console.log("msg. received.....")
	if (msg.type == "updateFreq"){
		globalFreq = msg.value;
	} else if(msg.type =="shimmer"){
		var freq = msg.freq;
		var sustain = msg.sustain;
		shimmerAudio(freq,4,1)
	} else if (msg.type == "solo"){
		console.log('solo given')
		window.alert("You've been given a solo for "+msg.dur+" seconds! You control the system now!")
		setTimeout(function(){
			window.alert("Times up rockstar, someone else's turn to jam.")
		},msg.dur*1000)
	}

})


// @look at what data produces 
// @long window
// @Spherical coordinate system
// @controll rate at which device motion is collected and sent

var xArray = [0,0,0,0,0,0,0,0,0,0]
var yArray = [0,0,0,0,0,0,0,0,0,0]
var zArray = [0,0,0,0,0,0,0,0,0,0]
var rMedium = [0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0] // n = 80
var rLong = [0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0] // n = 160
var rShort = [0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]

var rShort = rMedium = rLong = 0;
var rValues = [];
// @timestamp it 
// ring buffer, garbage collection
// time averages instaed
// window.onFocus = function() {
// 	isActive = true;
// 	console.log('focused')
// }

// window.onBlur= function (){
// 	isActive = false;
// 	console.log('not focused')
// }
// var max =0;
var nudgeLag = 0;
window.addEventListener('devicemotion', function(event) {
	
	// If they've hit 'start'
	if (isActive){
		var t = (new Date()).getTime();
		var xf = event.accelerationIncludingGravity.x
		var yf = event.accelerationIncludingGravity.y
		var zf = event.accelerationIncludingGravity.z
		var r = Math.sqrt(xf*xf+yf*yf+zf*zf)
		rValues.push([r,t])
		//@a more accurate/correctional number of array entries to keep than this...
		rValues = rValues.slice(-700);
		// rValues = rValues.slice((-1)*(rValues.length-(rLongIndex+1)))

		// if (r>max){
		// 	max = r;
		// }

		// console.log(max);
		//@ change this back to 60
		if (Math.abs(r)>15 && (t > nudgeLag)){
			var m = Math.max(Math.abs(xf),Math.abs(yf),Math.abs(zf))
			if (m==Math.abs(xf) )  {
				nudge(xf,0,0)
			} else if (m==Math.abs(yf)) {
				nudge(0,yf,0);
			} else if (m==Math.abs(zf)){
				nudge(0,0,zf)
			} else{
				console.log("Error detecting nudge")
			}
			//new nudges can only occur every quarter (prevents 'kickback' when device is decelerated)
			nudgeLag = t +250
			shimmerAudio(globalFreq,1,1)
		}
	}

});

setInterval(function(){
	// If they're in the game
	if (isActive){
		var t = (new Date()).getTime();
		var rShort = rMedium = rLong = 0;
		var rLongAv= rMediumAv = rShortAv = 0;
		var rLongIndex = rMedIndex = rShortIndex = 0;

		for (i in rValues){
			var val = rValues[i][0]
			//if the time stamp of that data is 10 secs old or younger,
			if (rValues[i][1]>=(t-10000)){
				rLong = rLong+val
				rLongIndex = rLongIndex+1
			}
			if (rValues[i][1]>=(t-5000)){
				rMedium = rMedium+val
				rMedIndex = rMedIndex+1
			}
			if (rValues[i][1]>=(t-2500)){
				rShort = rShort +val
				rShortIndex = rShortIndex+1
			}
		}
		
		//Get the average from the sums over the specified windows
		rInstant = rValues[rValues.length-1][0]-9.81;
		rLongAv = rLong/rLongIndex - 9.81
		rMediumAv = rMedium/rMedIndex - 9.81
		rShortAv = rShort/rShortIndex - 9.81
		
		//Scaling/normalizing and clipping
		rInstant = Math.min(Math.max(0,(Math.round(rInstant*10)/10)/90),1)
		rShortAv = Math.min(Math.max(0,(Math.round(rShortAv*10)/10)/65),1)
		rMediumAv = Math.min(Math.max(0,(Math.round(rMediumAv*10)/10)/60),1)
		rLongAv = Math.min(Math.max(0,(Math.round(rLongAv*10)/10)/55),1)

		// So don't get NaN values when someone leaves their browser/locks their phone
		if (isNaN(rInstant)) rInstant = 0;
		if (isNaN(rLongAv)) rLongAv = 0;
		if (isNaN(rMediumAv)) rMediumAv = 0;
		if (isNaN(rShortAv)) rShortAv = 0;

		//so we don't send undefined values at the startup...
		if (rValues[rValues.length-1]==undefined) return;
		
		scale = (rLongAv*4+rMediumAv*3+rShortAv*2+rInstant)/10;
		
		if (rLongAv>0.85){
			
			giveSolo()
			
		}

		var msg = {
			type: 'motion',
			xyz: [6,6,6],
			motion: rInstant,
			motionShort: rShortAv,
			motionMedium: rMediumAv,
			motionLong: rLongAv
		}
		try {
			ws.send(JSON.stringify(msg));
		} catch(e){
			console.log('error sending motion messages')
		}
	}
},50);



setInterval(function(){
	if (isActive){
		var msg = {
			type:"updateScale",
			scale: scale
		}
		ws.send(JSON.stringify(msg))
	}
},100)



setInterval(function(){
	if(isActive){
		var msg = {
			type:"updateL3s",
			diversity:document.getElementById('diversity').value,
			excitement:document.getElementById('excitement').value,
			depth:document.getElementById('depth').value,
		}
		ws.send(JSON.stringify(msg))
	}
},150)

