using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class AntiAir : BaseTower
{
	public override string TowerName => "Anti Aircraft";
	public override string TowerDesc => "A tower designed to target air targets";

	//Temporary until we get a pistol model
	public override string TowerModel => "models/towers/antiairtower.vmdl";
	public override int UnlockLevel => 30;
	public override BaseTower RequiredTowers => null;
	public override string[] TowerLevelDesc => new[]
	{
		"",
		"Anti aircraft with increased fire-rate and range",
		"High ranged anti aircraft",
		"Aircraft should steer clear of this tower",
	};

	public override List<(float AttTime, float AttDMG, int NewRange)> Upgrades => new()
	{
		new(-0.25f, 5.0f, 25),
		new(-0.5f, 5.0f, 65),
		new(-0.75f, 7.5f, 135),
		new(-1.0f, 7.5f, 175)
	};

	public override int TowerMaxLevel => 4;
	public override int TowerCost => 120;
	public override int[] TowerLevelCosts => new[]
	{
		135,
		175,
		265,
		-1,
	};

	public override float DeploymentTime => 3.72f;
	public override float AttackTime { get; set; } = 3.5f;
	public override float AttackDamage { get; set; } = 8.0f;
	public override int RangeDistance { get; set; } = 125;
	public override string AttackSound => "pistol_fire";
	public override bool CounterAirborne { get; set; } = true;

	[Event.Tick.Server]
	public override void SimulateTower()
	{
		base.SimulateTower();
	}

	public override BaseNPC ScanForEnemy( BaseNPC ignoreNPC = null )
	{
		var npc = base.ScanForEnemy( ignoreNPC );

		if ( npc != null && npc.AssetFile.NPCType != BaseNPCAsset.SpecialType.Airborne )
			return null;

		return npc;
	}

	[ClientRpc]
	public override void FireEffects()
	{
		base.FireEffects();

		Particles.Create( "particles/bullet_muzzleflash.vpcf", this, "muzzle" );
	}
}
