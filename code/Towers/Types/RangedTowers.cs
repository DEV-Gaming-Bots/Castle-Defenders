using Sandbox;
using System.Collections.Generic;

public sealed partial class Pistol : BaseTower
{
	public override string TowerName => "Pistol";
	public override string TowerDesc => "A very simple pistol tower";

	//Temporary until we get a pistol model
	public override string TowerModel => "models/towers/pistol_tower.vmdl";
	public override int UnlockLevel => 0;
	public override BaseTower RequiredTowers => null;
	public override string[] TowerLevelDesc => new[]
	{
		"",
		"A pistol with an increased range, damage and fire-rate",
		"A pistol with improved range, damage and fire-rate than previously",
		"An enhanced pistol with even better range, damage and fire-rate",
		"A heavily enhanced, top of the line pistol with best range, damage and fire-rate"
	};

	public override List<(float AttTime, float AttDMG, int NewRange)> Upgrades => new()
	{
		new(-0.25f, 1.25f, 0),
		new(-0.25f, 1.50f, 25),
		new(-0.50f, 1.75f, 25),
		new(-0.75f, 2.25f, 25),
		new(-1.0f, 2.75f, 50)
	};

	public override string[] TowerUpgradeDesc => new[]
	{
		$"Attack Speed +{-Upgrades[0].AttTime} | Damage +{Upgrades[0].AttDMG}",
		$"Attack Speed +{-Upgrades[1].AttTime} | Damage +{Upgrades[1].AttDMG} | Range +{Upgrades[1].NewRange}",
		$"Attack Speed +{-Upgrades[2].AttTime} | Damage +{Upgrades[2].AttDMG} | Range +{Upgrades[2].NewRange}",
		$"Attack Speed +{-Upgrades[3].AttTime} | Damage +{Upgrades[3].AttDMG} | Range +{Upgrades[3].NewRange}",
		$"Attack Speed +{-Upgrades[4].AttTime} | Damage +{Upgrades[4].AttDMG} | Range +{Upgrades[4].NewRange}",
		"",
	};

	public override int TowerMaxLevel => 5;
	public override int TowerCost => 20;
	public override int[] TowerLevelCosts => new[]
	{
		25,
		45,
		90,
		145,
		-1,
	};

	public override float DeploymentTime => 3.72f;
	public override float AttackTime { get; set; } = 3.5f;
	public override float AttackDamage { get; set; } = 7.5f;
	public override int RangeDistance { get; set; } = 125;
	public override string AttackSound => "pistol_fire";

	[Event.Tick.Server]
	public override void SimulateTower()
	{
		base.SimulateTower();

		if ( Target != null && Target.IsValid() )
			SetAnimParameter( "v_forward", Target.Position );

		SetAnimParameter( "b_attack", (TimeLastAttack + 0.1f) >= AttackTime && Target != null );
	}

	[ClientRpc]
	public override void FireEffects()
	{
		base.FireEffects();

		Particles.Create( "particles/bullet_muzzleflash.vpcf", this, "muzzle" );
	}
}

public sealed partial class SMG : BaseTower
{
	public override string TowerName => "SMG";
	public override string TowerDesc => "A fast shooting submachine tower";
	public override string TowerModel => "models/towers/smgtower.vmdl";
	public override int UnlockLevel => 0;
	public override BaseTower RequiredTowers => null;
	public override string[] TowerLevelDesc => new[]
	{
		"",
		"A two barrel SMG for more damage along with better firing",
		"A more improved version with a spinning barrel",
		"Now there are 4 total spinning barrels, who are they to judge"
	};

	public override List<(float AttTime, float AttDMG, int NewRange)> Upgrades => new()
	{
		new(-0.1f, 0.5f, 0),
		new(-0.1f, 1.0f, 25),
		new(-0.15f, 1.5f, 25),
		new(-0.25f, 2.25f, 25)
	};

	public override string[] TowerUpgradeDesc => new[]
	{
		$"Attack Speed +{-Upgrades[0].AttTime} | Damage +{Upgrades[0].AttDMG}",
		$"Attack Speed +{-Upgrades[1].AttTime} | Damage +{Upgrades[1].AttDMG} | Range +{Upgrades[1].NewRange}",
		$"Attack Speed +{-Upgrades[2].AttTime} | Damage +{Upgrades[2].AttDMG} | Range +{Upgrades[2].NewRange}",
		$"Attack Speed +{-Upgrades[3].AttTime} | Damage +{Upgrades[3].AttDMG} | Range +{Upgrades[3].NewRange}",
		"",
	};

	public override int TowerMaxLevel => 4;
	public override int TowerCost => 45;
	public override int[] TowerLevelCosts => new[]
	{
		60,
		180,
		360,
		-1,
	};

	public override float DeploymentTime => 3.72f;
	public override float AttackTime { get; set; } = 0.75f;
	public override float AttackDamage { get; set; } = 4.25f;
	public override int RangeDistance { get; set; } = 100;
	public override string AttackSound => "pistol_fire";

	[Event.Tick.Server]
	public override void SimulateTower()
	{
		base.SimulateTower();

		if ( Target != null && Target.IsValid() )
			SetAnimParameter( "v_forward", Target.Position );

		SetAnimParameter( "b_attack", (TimeLastAttack + 0.1f) >= AttackTime && Target != null );
	}

	[ClientRpc]
	public override void FireEffects()
	{
		base.FireEffects();

		Particles.Create( "particles/bullet_muzzleflash.vpcf", this, "muzzle_l" );
		Particles.Create( "particles/bullet_muzzleflash.vpcf", this, "muzzle_r" );
	}
}

public sealed partial class Sniper : BaseTower
{
	public override string TowerName => "Sniper";
	public override string TowerDesc => "A sniper tower that can fire from a large distance";

	//Temporary until we get a pistol model
	public override string TowerModel => "models/towers/snipertower.vmdl";
	public override int UnlockLevel => 0;
	public override BaseTower RequiredTowers => null;
	public override int[] TowerLevelCosts => new[]
	{
		145,
		235,
		295,
		400,
		500,
		-1
	};

	public override string[] TowerLevelDesc => new[]
	{
		"",
		"Upgraded sniper tower with better sniper power",
		"Modified sniper tower with more powerful sniping",
		"Enhanced sniper tower with better sniping power and equipped with a infra-red scope",
		"This tower can snipe incredibly fast and large distances",
		"This tower is dangerously lethal even from a far distance"
	};
	public override List<(float AttTime, float AttDMG, int NewRange)> Upgrades => new()
	{
		new(-0.25f, 2.5f, 25),
		new(-0.30f, 2.5f, 25),
		new(-0.45f, 2.5f, 25),
		new(-0.50f, 5.0f, 50),
		new(-0.50f, 5.0f, 50),
		new(-0.50f, 7.5f, 100)
	};
	public override string[] TowerUpgradeDesc => new[]
	{
		$"Attack Speed +{-Upgrades[0].AttTime} | Damage +{Upgrades[0].AttDMG} | Range +{Upgrades[0].NewRange}",
		$"Attack Speed +{-Upgrades[1].AttTime} | Damage +{Upgrades[1].AttDMG} | Range +{Upgrades[1].NewRange}",
		$"Attack Speed +{-Upgrades[2].AttTime} | Damage +{Upgrades[2].AttDMG} | Range +{Upgrades[2].NewRange} | Enables counter-cloak technology",
		$"Attack Speed +{-Upgrades[3].AttTime} | Damage +{Upgrades[3].AttDMG} | Range +{Upgrades[3].NewRange}",
		$"Attack Speed +{-Upgrades[4].AttTime} | Damage +{Upgrades[4].AttDMG} | Range +{Upgrades[4].NewRange}",
		"",
	};

	public override int TowerMaxLevel => 6;
	public override int TowerCost => 125;
	public override float DeploymentTime => 3.75f;
	public override float AttackTime { get; set; } = 9.5f;
	public override float AttackDamage { get; set; } = 35.0f;
	public override int RangeDistance { get; set; } = 250;
	public override string AttackSound => "sniper_fire";

	private bool _lockedOnTarget;
	private Particles _laserSight;

	public override void UpgradeTower()
	{
		base.UpgradeTower();

		if ( TowerLevel == 4 )
			CounterStealth = true;
	}

	protected override void OnDestroy()
	{
		LaserOff(To.Everyone);
		base.OnDestroy();
	}

	public override void SellTower()
	{
		LaserOff();
		base.SellTower();
	}

	[ClientRpc]
	public override void FireEffects()
	{
		base.FireEffects();
	}

	[Event.Tick.Server]
	public override void SimulateTower()
	{
		base.SimulateTower();

		if ( Target == null )
		{
			_lockedOnTarget = false;
			LaserOff(To.Everyone);

			return;
		}

		if ( TimeLastAttack * 4 >= AttackTime && !_lockedOnTarget )
		{
			LaserOn( To.Everyone, Target );
			_lockedOnTarget = true;
		}
	}

	[ClientRpc]
	private void LaserOn( BaseNPC target )
	{
		Host.AssertClient();

		if ( target == null )
			return;

		_laserSight = Particles.Create( "particles/sniper_beam.vpcf" );
		_laserSight.SetEntityAttachment( 1, this, "muzzle" );
		_laserSight.SetEntity( 0, target, Vector3.Up * 25 );
	}

	[ClientRpc]
	private void LaserOff()
	{
		Host.AssertClient();

		if ( _laserSight != null )
		{
			_laserSight.Destroy( true );
			_laserSight = null;
		}
	}

	public override void Attack( BaseNPC target )
	{
		LaserOff();

		_lockedOnTarget = false;

		base.Attack( target );
	}
}
