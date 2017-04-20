(
Pdefn(\degree, Pstutter(Pfunc({(-9*(~diversity)+10).round;}),Pseq([Pfunc({
			var val=0;

			val = case
			{~diversity >= 0.9} {val = ~scale.degrees.choose}
			{~diversity >= 0.8} {val = (~scale.degrees.choose)%(~scale.degrees.size-((~scale.degrees.size/6).floor))}
			{~diversity >= 0.7} {val = (~scale.degrees.choose)%(~scale.degrees.size-((~scale.degrees.size/5).floor))}
			{~diversity >= 0.6} {val = (~scale.degrees.choose)%(~scale.degrees.size-((~scale.degrees.size/4).floor))}
			{~diversity >= 0.5} {val = (~scale.degrees.choose)%(~scale.degrees.size-((~scale.degrees.size/3).floor))}
			{~diversity >= 0.4} {val = (~scale.degrees.choose)%(~scale.degrees.size-((~scale.degrees.size/2).floor))}
			{true} {val=0;};
			val;
})],inf)));
);


// Inputs: motion, coherence

// Outputs: Excitement, Diversity, Depth
// Means: synths and tempo





(
Pdef(\bass,
	Pbind(
		\instrument,\bass,
		\octave, 4+Pfunc({
			if (~diversity<0.4, 0,
				({ [ 0, -1].wchoose([1,~diversity*(~depth/2+1)].normalizeSum)}));
		}),
		\degree,Pdefn(\degree),
		\dur,Pseq([Pfunc({(((0.125-1)*~excitement+1)*4).round/4})],inf),
		\modMul,Pseq((1,2..16)++(1,2..16).reverse,inf)/Pfunc({(-7*~depth+8).clip(1,8)}),
		\legato, Pfunc({(-1)*~excitement+2}),
		\at, 0.01,
		\db,Pfunc({
			case
			{~excitement>=0.87} {-16}
			{~excitement>=0.77} {-18}
			{~excitement>=0.67} {-20}
			{~excitement>=0.57} {-21}
			{~excitement>=0.47} {-22}
			{true} {-22}

		})+2
	) <> Pdef(\master);
).play;
);


(
Pdef(\hihat,
	Pbind(
		\instrument,\hihat,
		\dur,Pstutter(4,Pfunc({
			case
			{~excitement>=0.8} {0.125}
			{~excitement>=0.4} {0.5}
			{~excitement>=0.3} {1}
			{~excitement>=0.2} {1}
			{~excitement>=0.1} {2}
			{~excitement>=0} {8}
			{true} {16}
		})),
		\midinote, Pseq([\rest,0],inf),
		\at, Pseq((0.001,0.005..0.05)++((0.001,0.005..0.05).reverse),inf)*Pfunc({~diversity.clip(0.001,1)}),
		\rt, 0.125,
		\out, Pfunc({
			if (~diversity<0.4, [0,1],
				[[0,1],0,1].wchoose([1,(~diversity/2+0.5)*(~excitement+1),(~diversity+0.5)*(~excitement+1)].normalizeSum));
		}),
	) <> Pdef(\master);
).stop;
Pdef(\hihat1,
	Pbind(
		\instrument,\hihat,
		\dur,0.5,
		\midinote, Pseq([\rest,0],inf),
		\at, Pseq((0.001,0.005..0.05)++((0.001,0.005..0.05).reverse),inf)*Pfunc({~diversity.clip(0.001,1)}),
		\rt, 0.125,
		\out, Pfunc({
			if (~diversity<0.4, [0,1],
				[[0,1],0,1].wchoose([1,(~diversity/2+0.5)*(~excitement+1),(~diversity+0.5)*(~excitement+1)].normalizeSum));
		}),
		\db,Pfunc({if(~excitement>0.18,-20,-200)})
	) <> Pdef(\master);
).play;

Pdef(\hihat2,
	Pbind(
		\instrument,\hihat,
		\dur,0.25,
		\midinote, Pseq([\rest,0],inf),
		\at, Pseq((0.001,0.005..0.05)++((0.001,0.005..0.05).reverse),inf)*Pfunc({~diversity.clip(0.001,1)}),
		\rt, 0.125,
		\out, Pfunc({
			if (~diversity<0.4, [0,1],
				[[0,1],0,1].wchoose([1,(~diversity/2+0.5)*(~excitement+1),(~diversity+0.5)*(~excitement+1)].normalizeSum));
		}),
		\db,Pfunc({if(~excitement>0.6 && (~excitement<0.8),-20,-200)})
	) <> Pdef(\master);
).play;

Pdef(\hihat3,
	Pbind(
		\instrument,\hihat,
		\dur,Pseq([0.25,0.125/2,0.125/2,0.125/2,0.125/2],inf),
		\midinote, Pseq([\rest,0,\rest,\rest,\rest,\rest,0,0,0,0],inf),
		\at, Pseq((0.001,0.005..0.05)++((0.001,0.005..0.05).reverse),inf)*Pfunc({~diversity.clip(0.001,1)}),
		\rt, 0.125,
		\out, Pfunc({
			if (~diversity<0.4, [0,1],
				[[0,1],0,1].wchoose([1,(~diversity/2+0.5)*(~excitement+1),(~diversity+0.5)*(~excitement+1)].normalizeSum));
		}),
		\db,Pfunc({if(~excitement>=0.8,-20,-200)})
	) <> Pdef(\master);
).play;
);



(
Pdef(\kick,
	Pbind(
		\instrument,\kick,
		\dur,Pstutter(2,Pfunc({
			if(~excitement>0.9,0.5,
				([1,0.5].wchoose([1,~excitement].normalizeSum)));
		})),
		\midinote,Pseq([30],inf)+Prand([0,0,0,0,0],inf),
		\db,Pfunc({
			case
			{~excitement>=0.9} {-15}
			{~excitement>=0.88} {-16}
			{~excitement>=0.74} {-17}
			{~excitement>=0.62} {-18}
			{~excitement>=50} {-19}
			{~excitement>=0.38} {-20}
			{~excitement>=0.26} {-22}
			{~excitement>=0.14} {-25}
			{true} {-200}
		})
	) <> Pdef(\master);
).play;
);



(
Pdef(\arpeg,
	Pbind(
		\instrument,\arpeg,
		\dur,Pfunc({((((-14)*~diversity+16)*0.5).round/0.5).clip(2,16)}),
		\octave,6,
		\degree,Pdefn(\degree)+Pfunc({
			([0,[0,7],[12,0]].wchoose([1,~diversity,~diversity].normalizeSum));
		}),
		\at, 2,
		\rt,1,
		\arpegFreq, Pfunc({(0.03125-0.5)*~diversity+0.5}),
		\db,Pfunc({((~excitement*~diversity)/2).linexp(0,1,-80,-10).clip(-100,-10)}),
		\legato,1.25,
		\out,0,
		\panScatter,Pseq((0.1,0.2..1)++((0.1,0.2..1).reverse),inf)*Pfunc({1+~diversity})
	) <> Pdef(\master);
).play;
);


(
Pdef(\snare,
	Pbind(
		\instrument,\snare,
		\dur,Pstutter(4,Pfunc({
			case
			{~excitement>=0.95} {0.25}
			{~excitement>=0.8} {0.5}
			{~excitement>=0.45} {1}
			{~excitement<0.45} {2};
		})),
		\midinote, Pseq([\rest,0],inf),
			\db,Pfunc({
			case
			{~excitement>=0.85} {-16}
			{~excitement>=0.75} {-18}
			{~excitement>=0.65} {-20}
			{~excitement>=0.55} {-22}
			{~excitement>=0.45} {-24}
			{~excitement>=0.35} {-26}
			{~excitement<0.35} {-200}
			{true} {-200}
			}),
		\lpf, Pseq((400,500..2000),inf)*Pfunc({~diversity+1})+Pfunc({(900-10000)*~depth + 10000})
	) <> Pdef(\master);
).play;
);
(
Pdef(\snare2,
	Pbind(
		\instrument,\snare,
		\dur,Pseq([0.25],inf),
		\midinote,Pseq([Pseq([\rest],60), \rest,0,\rest,0],inf),
		\out,[0,1],
		\db,Pfunc({
			if(~excitement>0.5,-20,-100);
		}),
		\lpf, Pfunc({(900-10000)*~depth + 10000})
	)
).play;
);


(
Pdef(\pad,
	Pbind(
		\instrument,\pad,
		\octave,5,
		\lfoFreq,Pfunc({
			case
			{~excitement>0.85} {8}
			{~excitement>0.72} {4}
			{~excitement>0.52} {2}
			{~excitement>0.32} {1}
			{~excitement>0.22} {0.5}
			{true} {0.25}

		})*2,
		\dur,Pfunc({
			([16,8,4,2].wchoose([1,~diversity, ~diversity*~diversity*2,~diversity*~diversity*3].normalizeSum));
		}),
		\out, 0,
		\panScatter, Pfunc({~diversity}),
		\degree, Pdefn(\degree),
		\at, 1,
		\rt,2,
		\db,-10
	) <> Pdef(\master);
).play;
);
