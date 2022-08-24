using System;
using System.Collections.Generic;
using Sandbox;


public partial class Lightning : BaseTower
{
	public override string TowerName => "Lightning";
	public override string TowerDesc => "A tower that has the power of thunder, quite shocking";
	public override string TowerModel => "models/towers/lightningtower.vmdl";
	public override int UnlockLevel => 0;
	public override BaseTower RequiredTowers => null;
	public override int[] TowerLevelCosts => new int[]
	{
		275,
		375,
		650,
		950,
		1350,
		-1
	};

	public override List<(float AttTime, float AttDMG, int NewRange)> Upgrades => new()
	{
		new(-0.10f, 10.0f, 0),
		new(-0.20f, 15.0f, 10),
		new(-0.35f, 17.5f, 15),
		new(-0.40f, 22.5f, 20),
		new(-0.45f, 25.0f, 25),
		new(-0.50f, 25.5f, 40),
	};

	public override string[] TowerLevelDesc => new string[]
	{
		"",
		"An improved version allowing more energic attacks",
		"An even more updated Lightning tower, Harnessing more power from the storms",
		"Quite the shocking upgrade, allowing for more damage",
		"Even more shocking power, you don't wanna mess with this",
		"Surprisingly harnesses the power of Thor's thunder, who would have thought"
	};

	public override int TowerMaxLevel => 6;
	public override int TowerCost => 225;
	public override float DeploymentTime => 2.25f;
	public override float AttackTime { get; set; } = 3.25f;
	public override float AttackDamage { get; set; } = 32.5f;
	public override int RangeDistance { get; set; } = 125;
	public override string AttackSound => "lightning_attack";

	bool charged = false;

	[Event.Tick.Server]
	public override void SimulateTower()
	{
		base.SimulateTower();

		if ( Target == null )
		{
			TimeLastAttack = 0;
			charged = false;
			return;
		}

		if ( (TimeLastAttack * 2) >= AttackTime && !charged )
		{
			PlaySound( "lightning_charge" );
			charged = true;
		}
	}

	public override void Attack( BaseNPC target )
	{
		base.Attack( target );
		charged = false;
	}

	[ClientRpc]
	public override void FireEffects()
	{
		if ( IsPreviewing )
			return;

		base.FireEffects();
	}
}
