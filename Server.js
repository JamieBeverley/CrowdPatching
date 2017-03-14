var http = require('http');
var express = require('express');
var server = http.createServer();
var expressServer = express();
var osc = require ('osc')

var scOSC = new osc.UDPPort({
	localAddress: "0.0.0.0", 
	localPort: 9000,
	remoteAddress: "127.0.0.1",
	remotePort: 9001
})
scOSC.open();

// uses current directory
expressServer.use(express.static(__dirname));
server.on('request', expressServer)


//server listening on 8000
server.listen(8000, function(){console.log("listening")})

// from this can make websocket server

var WebSocket = require('ws')
var wsServer = new WebSocket.Server({server: server});

var id=0;
var clients = {};
var close = {}
var numClients=0;

wsServer.on('connection', function(r){
	id=id+1;
	console.log("_____________________ ID:  "+id)
	
	var newMsg = {
		address: "/newAudienceMember",
		args:[id,"User"]
	}
	scOSC.send(newMsg);

	numClients++;
	
	r.identifier=id;

	r.closingFlag=false;
	//Default setting
	clients[id]={
		id: id, 
		xyz:[0,0,0], 
		motion:0, 
		motionShort:0, 
		motionMedium:0, 
		motionLong:0,
		username:"User"+id,
		scale:0,
		diversity:0,
		excitement:0,
		depth:0
	};

	console.log((new Date())+ 'Connection accepted, id: '+ id);


	r.on('message',function(message){

		var msg = JSON.parse(message);
		if (msg.type =='motion'){
			//Sends motion messages
			try{
				clients[r.identifier].xyz = [msg.xyz[0],msg.xyz[1],msg.xyz[2]]

				clients[r.identifier].motion = msg.motion;
				clients[r.identifier].motionShort = msg.motionShort
				clients[r.identifier].motionMedium = msg.motionMedium 
				clients[r.identifier].motionLong = msg.motionLong

				// console.log("#uncooked:  " +msg.motion)
			}
			catch(e){
				console.log("WARNING: motion update dropped for: "+id)
				console.log(e)
				console.log("________________________________________")
			}
		} else if (msg.type == 'updateWeights'){
			try{
				clients[r.identifier].scale = msg.scale;
				clients[r.identifier].diversity = msg.diversity;
				clients[r.identifier].excitement = msg.excitement;
				clients[r.identifier].depth = msg.depth;

			}
			catch (e){
				console.log("WARNING: updateWeights msg dropped for"+id)
				console.log(e)
				console.log("________________________________________")
			}

		} else{
				console.log("WARNING: Server received unhandled message time from clients")
				console.log("________________________________________")

		}
	});//end on message

	r.on('error',function(){
		for (var a in clients){
			if (clients[a].client.identifier == r.identifier) delete clients[a]
	}
	});//end on 'error'

	r.on('close', function(reasonCode, description){
		for (var a in clients){
			if (clients[a].client.identifier==r.identifier) {
				// if (clients[a].team=="Blue") blueTeam--; else redTeam--;
				close[a]=true;
				console.log("Client: " +a+" disconnected");
				closeMsg = {
					address:"/removeAudienceMember",
					args: [r.identifier]
				}
				try {
					scOSC.send(closeMsg);
				} catch (e)
				{
					console.log('error sending removeAudienceMember to SC')
				}
				delete clients[a]
				break;
			}
		}
		numClients--;

		//Tell SuperCollider how many clients are connected
		scOSC.send({address:"/disconnect", args: []});
	});// end on 'close'
});

setInterval(function(){
	var motionArray = []
	var motionShortArray = []
	var motionMediumArray = []
	var motionLongArray = []


	for (var ids in clients){
		var i = clients[ids]
		motionArray.push(parseFloat(i.motion));
		motionShortArray.push(parseFloat(i.motionShort));
		motionMediumArray.push(parseFloat(i.motionMedium));
		motionLongArray.push(parseFloat(i.motionLong));
	}	

	sendClientData();
	sendMotionData(motionArray, motionShortArray, motionMediumArray, motionLongArray);

},100)


function sendClientData(){
	for (var ids in clients){
		var i = clients[ids];
		updateMsg = {
			address: "/updateAudience",
			args:[i.id,i.username,i.scale,i.diversity, i.excitement,i.depth]
		}

		try{
			scOSC.send(updateMsg)
		} catch (e){
			console.log("WARNING: update msg not sent for"+ids)
			console.log(e)
			console.log("________________________________________")
		}

	}

}


//@ Long dimension to variance: what kind of variance is happening over the past x Long?
// ex: mean over window, max and min over window, slope over window, etc...
//function sendData(motionArray, motionShortArray, motionMediumArray, motionLongArray, xArray, yArray, zArray, team){

function sendMotionData(motionArray, motionShortArray, motionMediumArray, motionLongArray){
	var motionMean = motionVariance = motionShortMean = motionShortVariance = motionMediumMean = motionMediumVariance = motionLongMean = motionLongVariance = 0//= xMean = xVariance = yMean = yVariance = zMean = zVariance = xVarLong=yVarLong=zVarLong= xVarShort=yVarShort=zVarShort= 0;

	//Calculates mean motion
	for (var val in motionArray){
		motionMean = motionMean + motionArray[val];
		motionShortMean = motionShortMean+motionShortArray[val]
		motionMediumMean = motionMediumMean+motionMediumArray[val]
		motionLongMean = motionLongMean + motionLongArray[val]
	}
	if(motionArray.length!=0) {
		motionMean = motionMean/motionArray.length
		motionMediumMean = motionMediumMean/motionMediumArray.length
		motionShortMean = motionShortMean/motionShortArray.length
		motionLongMean = motionLongMean/motionLongArray.length
	}
	else {motionMean = motionShortMean = motionLongMean = motionMediumMean = 0;}

	//Calculates motion variance
	for (var val in motionArray){
		motionVariance = motionVariance+(motionArray[val]-motionMean)*(motionArray[val]-motionMean)
		motionShortVariance = motionShortVariance + (motionShortArray[val]-motionShortMean)*(motionShortArray[val]-motionShortMean)
		motionMediumVariance = motionMediumVariance + (motionMediumArray[val]-motionMediumMean)*(motionMediumArray[val]-motionMediumMean)
		motionLongVariance = motionLongVariance + (motionLongArray[val]-motionLongMean)*(motionLongArray[val]-motionLongMean)
	}	
	if(motionArray.length!=0) {
		motionVariance = motionVariance/motionArray.length; 
		motionShortVariance=motionShortVariance/motionShortArray.length;
		motionMediumVariance = motionMediumVariance/motionMediumArray.length;
		motionLongVariance= motionLongVariance/motionLongArray.length;
	}
	else {motionVariance = motionShortVariance = motionLongVariance = motionMediumVariance = 0;}
	
	//Normalize it.
	// motionMean = Math.min((motionMean)/30,1)
	// motionShortMean = Math.min((motionShortMean)/30,1)
	// motionLongMean = Math.min((motionLongMean)/30,1)

	motionMean = Math.min(Math.max(0,(Math.round(motionMean*10)/10)/90),1)
	motionShortMean = Math.min(Math.max(0,(Math.round(motionShortMean*10)/10)/65),1)
	motionMediumMean = Math.min(Math.max(0,(Math.round(motionMediumMean*10)/10)/60),1)
	motionLongMean = Math.min(Math.max(0,(Math.round(motionLongMean*10)/10)/55),1)


	console.log("################################")

	console.log("motionMean:  "+motionMean)
	console.log("motionShortMean:  "+motionShortMean)
	console.log("motionMediumMean:  "+motionMediumMean)
	console.log("motionLongMean:  "+motionLongMean)
	console.log("motion Variance: "+motionVariance)
	console.log("motionShortVariance:  "+motionShortVariance)
	console.log("motionMediumVariance:  "+motionMediumVariance)
	console.log("motionLongVariance:  "+motionLongVariance)
	console.log("-------------------------------");

	var coh = (1/motionVariance)*0.5*(1+motionMean)+(1/motionShortVariance)*0.4*(1+motionShortMean)+(1/motionMediumVariance)*0.2*(1+motionMediumMean)+(1/motionLongVariance)*0.15*(1.2+motionLongMean)

	var coherenceMsg = {
		address: "/coherence",
		args: coh
	}

	var avgMotion = (motionMean+motionShortMean+motionLongMean+motionMediumMean)/4
	var motionMsg = {
		address:"/motion",
		args: [avgMotion]
	}
	

	try{
		scOSC.send(coherenceMsg);
		scOSC.send(motionMsg)
	}
	catch(e){
		console.log("***********error sending OSC****************")
		console.log(e)
		console.log("______________________")
	}
}



function mean(array){
	var result=0
	for (i in array){
		result=result+array[i]
	}
	return result/array.length
}


scOSC.on('message',function(msg){
	var msg;
	var team;
	var type;
	for (var i in clients) {
		console.log("id:  "+i+"   color:  "+clients[i].team)
	}
	
	switch (msg.address[7]){
		case 'b':
			team = "Blue"
			break;
		case 'g':
			team = "Green"
			break;
		case 'r':
			team = "Red"
			break;
	}
	if (msg.address.endsWith("cheering")) {type = "cheering";}
	else if (msg.address.endsWith("loudness")) {type = "loudness"}
	else if (msg.address.endsWith("penalty")) {type = "penalty"}
	else if (msg.address.endsWith("pitch")) {type = "pitch"}
	else if (msg.address.endsWith("vibration")) {type = "vibration"}
	else return;
	
	var val = parseFloat(msg.args);

	var wsMsg = {type: type, value: val}

	wsMsg=JSON.stringify(wsMsg)

	if (team=="Green"){
		for (var i in clients){
			clients[i].client.send(wsMsg)
			console.log("Green")
		}
	}
	else if(team=="Blue"){
		for (var i in clients){
			console.log("Blue")
			if (clients[i].team == "Blue") clients[i].client.send(wsMsg)
		}
	}

	else if(team=="Red"){
		for (var i in clients){
			console.log("Red")
			if (clients[i].team == "Red") clients[i].client.send(wsMsg)
		}
	}
})

wsServer.broadcast = function (data){
  for (i in clients)
    i.send(data)

}


