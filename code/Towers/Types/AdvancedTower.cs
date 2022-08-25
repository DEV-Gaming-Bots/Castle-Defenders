﻿using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

public partial class RadioactiveEmitter : BaseTower
{
	public override string TowerName => "Radioactive Emitter";
	public override string TowerDesc => "A tower that emits radiation to nearby hostiles";

	//Temporary until we get a pistol model
	public override string TowerModel => "models/towers/radioactivemitter.vmdl";
	public override int UnlockLevel => 0;
	public override BaseTower RequiredTowers => null;
	public override string[] TowerLevelDesc => new string[]
	{
		"",
		"Emits even deadier radiation",
		"Emits serious radiation damage",
		"Best not to get too close or you're done for",
		"Now classed as a Chernobyl diaster... for them",
	};
	public override int[] TowerLevelCosts => new int[]
	{
		425,
		650,
		800,
		1250,
		-1,
	};

	public override List<(float AttTime, float AttDMG, int NewRange)> Upgrades => new()
	{
		new(-0.25f, 1.5f, 25),
		new(-0.5f, 1.5f, 50),
		new(-1.0f, 1.75f, 75),
		new(-1.0f, 2.25f, 75),
		new(-1.0f, 3.50f, 75)
	};

	public override int TowerMaxLevel => 5;
	public override int TowerCost => 350;
	public override float DeploymentTime => 3.67f;
	public override float AttackTime => 7.0f;
	public override float AttackDamage => 7.25f;
	public override int RangeDistance => 165;
	public override string AttackSound => "geiger_radiation";

	public override void Spawn()
	{
		base.Spawn();
	}

	[Event.Tick.Server]
	public override void SimulateTower()
	{
		if ( IsPreviewing )
			return;

		//Still deploying, wait until finished
		if ( TimeSinceDeployed < DeploymentTime )
			return;

		if ( TimeLastAttack < AttackTime )
			return;

		ScanForEnemies().ToList().ForEach( x => Attack( x ) );
	}

	[ClientRpc]
	public override void FireEffects()
	{
		base.FireEffects();
	}
}
