using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

public sealed partial class RadioactiveEmitter : BaseTower
{
	public override string TowerName => "Radiation Emitter";
	public override string TowerDesc => "A tower that emits radiation to nearby hostiles even with stealth";
	public override string TowerModel => "models/towers/radioactivemitter.vmdl";
	public override int UnlockLevel => 0;
	public override BaseTower RequiredTowers => null;
	public override string[] TowerLevelDesc => new[]
	{
		"",
		"Emits even deadier radiation",
		"Emits serious radiation damage",
		"Best not to get too close or you're done for",
		"Now classed as a Chernobyl diaster... for them",
	};
	public override int[] TowerLevelCosts => new[]
	{
		425,
		650,
		800,
		1250,
		-1,
	};

	public override List<(float AttTime, float AttDMG, int NewRange)> Upgrades => new()
	{
		new(-0.25f, 1.75f, 25),
		new(-0.5f, 1.75f, 50),
		new(-1.0f, 2.25f, 75),
		new(-1.0f, 2.75f, 75),
		new(-1.0f, 3.50f, 75)
	};

	public override string[] TowerUpgradeDesc => new[]
	{
		$"Attack Speed +{-Upgrades[0].AttTime} | Damage +{Upgrades[0].AttDMG} | Range +{Upgrades[0].NewRange}",
		$"Attack Speed +{-Upgrades[1].AttTime} | Damage +{Upgrades[1].AttDMG} | Range +{Upgrades[1].NewRange}",
		$"Attack Speed +{-Upgrades[2].AttTime} | Damage +{Upgrades[2].AttDMG} | Range +{Upgrades[2].NewRange}",
		$"Attack Speed +{-Upgrades[3].AttTime} | Damage +{Upgrades[3].AttDMG} | Range +{Upgrades[3].NewRange}",
		$"Attack Speed +{-Upgrades[4].AttTime} | Damage +{Upgrades[4].AttDMG} | Range +{Upgrades[4].NewRange}",
		"",
	};
	public override int TowerMaxLevel => 5;
	public override int TowerCost => 350;
	public override float DeploymentTime => 3.67f;
	public override float AttackTime { get; set; } = 7.0f;
	public override float AttackDamage { get; set; } = 15.0f;
	public override int RangeDistance { get; set; } = 165;

	public override string AttackSound => "geiger_radiation";
	public override bool CounterStealth { get; set; } = true;
	
	//TEMPORARY
	public override bool CounterAirborne { get; set; } = true;

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


public sealed partial class Trickster : BaseTower
{
	public override string TowerName => "Trickster";
	public override string TowerDesc => "A tower that has a chance of confusing the target into walking backwards";
	public override string TowerModel => "models/towers/trickstertower.vmdl";
	public override int UnlockLevel => 0;
	public override BaseTower RequiredTowers => null;
	public override string[] TowerLevelDesc => new[]
	{
		"",
		"Has a better chance of confusing enemies whilst dealing more damage",
		"Does even more damage and a stronger chance of confusion",
		"Enemies are more easily fooled into going backwards",
		"Well... I don't think enemies will have an idea what hit them",
	};
	public override int[] TowerLevelCosts => new[]
	{
		350,
		465,
		725,
		-1,
	};

	public override List<(float AttTime, float AttDMG, int NewRange)> Upgrades => new()
	{
		new(-0.25f, 2.25f, 25),
		new(-0.5f, 2.5f, 50),
		new(-1.0f, 2.75f, 75),
		new(-1.0f, 3.0f, 75)
	};

	public override string[] TowerUpgradeDesc => new[]
	{
		$"Attack Speed +{-Upgrades[0].AttTime} | Damage +{Upgrades[0].AttDMG} | Range +{Upgrades[0].NewRange} | Confusion Chance +5%",
		$"Attack Speed +{-Upgrades[1].AttTime} | Damage +{Upgrades[1].AttDMG} | Range +{Upgrades[1].NewRange} | Confusion Chance +5%",
		$"Attack Speed +{-Upgrades[2].AttTime} | Damage +{Upgrades[2].AttDMG} | Range +{Upgrades[2].NewRange} | Confusion Chance +7.5%",
		$"Attack Speed +{-Upgrades[3].AttTime} | Damage +{Upgrades[3].AttDMG} | Range +{Upgrades[3].NewRange} | Confusion Chance +12.5%",
		"",
	};

	public override int TowerMaxLevel => 4;
	public override int TowerCost => 275;
	public override float DeploymentTime => 3.67f;
	public override float AttackTime { get; set; } = 6.25f;
	public override float AttackDamage { get; set; } = 10.0f;
	public override int RangeDistance { get; set; } = 125;
	public override string AttackSound => "trickster_fire";
	public double ConfusionChance { get; set; } = 10.0;

	public override void Spawn()
	{
		base.Spawn();
		NetStats = $"Attack Delay {MathF.Round( AttackTime, 2 )} | Damage {AttackDamage} | Range {RangeDistance} | Effect Chance {ConfusionChance}%";
	}

	[Event.Tick.Server]
	public override void SimulateTower()
	{
		base.SimulateTower();
	}

	public override void UpgradeTower()
	{
		switch ( TowerLevel )
		{
			case 1:
				ConfusionChance += 5;
				break;
			case 2:
				ConfusionChance += 5;
				break;
			case 3:
				ConfusionChance += 7.5;
				break;
			case 4:
				ConfusionChance += 12.5f;
				break;
		}

		base.UpgradeTower();

		NetStats = $"Attack Delay {MathF.Round( AttackTime, 2 )} | Damage {AttackDamage} | Range {RangeDistance} | Effect Chance {ConfusionChance}%";
	}

	public override void Attack( BaseNPC target )
	{
		base.Attack( target );

		var time = 2.5f * TowerLevel;

		if ( ConfusionChance >= Rand.Double( 0.0, 100.0 ) )
		{
			target.AddEffect( BaseNPC.EffectEnum.Confusion, time );
			target.PlaySound( "confusion_apply" );
		}
	}

	[ClientRpc]
	public override void FireEffects()
	{
		base.FireEffects();
	}
}
