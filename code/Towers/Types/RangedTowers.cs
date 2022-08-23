using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

public partial class Pistol : BaseTower
{
	public override string TowerName => "Pistol Tower";
	public override string TowerDesc => "A very simple pistol tower";

	//Temporary until we get a pistol model
	public override string TowerModel => "models/towers/trickstertower.vmdl";
	public override int UnlockLevel => 0;
	public override BaseTower RequiredTowers => null;
	public override string[] TowerLevelDesc => new string[]
	{
		"",
		"A pistol with an increased range, damage and fire-rate",
		"A pistol with improved range, damage and fire-rate than previously",
		"An enhanced pistol with even better range, damage and fire-rate",
		"A heavily enhanced, top of the line pistol with best range, damage and fire-rate"
	};

	public override List<(float AttTime, float AttDMG, int NewRange)> Upgrades => new()
	{
		new(-0.10f, 0.5f, 0),
		new(-0.20f, 0.75f, 25),
		new(-0.35f, 1.0f, 25),
		new(-0.65f, 1.50f, 25),
		new(-0.80f, 2.0f, 50)
	};

	public override int TowerMaxLevel => 5;
	public override int TowerCost => 15;
	public override int[] TowerLevelCosts => new int[]
	{
		30,
		50,
		85,
		115,
		-1,
	};

	public override float DeploymentTime => 3.72f;
	public override float AttackTime { get; set; } = 3.0f;
	public override float AttackDamage { get; set; } = 5.0f;
	public override int RangeDistance { get; set; } = 125;
	public override string AttackSound => "pistol_fire";

	[ClientRpc]
	public override void FireEffects()
	{
		base.FireEffects();
	}
}

public partial class Sniper : BaseTower
{
	public override string TowerName => "Sniper Tower";
	public override string TowerDesc => "A sniper tower that can fire from a large distance";

	//Temporary until we get a pistol model
	public override string TowerModel => "models/towers/snipertower.vmdl";
	public override int UnlockLevel => 0;
	public override BaseTower RequiredTowers => null;
	public override int[] TowerLevelCosts => new int[]
	{
		150,
		225,
		300,
		475,
		750,
		-1
	};

	public override string[] TowerLevelDesc => new string[]
	{
		"",
		"Upgraded sniper tower with better sniper power",
		"Enhanced sniper tower with better sniping power",
		"Heavily modified sniper tower with more powerful sniping",
		"This tower can snipe incredibly fast and large distances",
		"This tower is dangerously lethal even from a far distance"
	};
	public override List<(float AttTime, float AttDMG, int NewRange)> Upgrades => new()
	{
		new(-0.25f, 5.0f, 25),
		new(-0.25f, 7.5f, 25),
		new(-0.35f, 10.0f, 25),
		new(-0.40f, 15.0f, 50),
		new(-0.50f, 15.0f, 50),
		new(-0.50f, 15.0f, 100)
	};

	public override int TowerMaxLevel => 6;
	public override int TowerCost => 100;
	public override float DeploymentTime => 3.75f;
	public override float AttackTime { get; set; } = 3.75f;
	public override float AttackDamage { get; set; } = 25.0f;
	public override int RangeDistance { get; set; } = 250;
	public override string AttackSound => "sniper_fire";

	bool lockedOnTarget;
	Particles laserSight;

	public override void Spawn()
	{
		base.Spawn();
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();
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
			TimeLastAttack = 0;
			lockedOnTarget = false;
			
			if ( laserSight != null )
			{
				laserSight.Destroy( true );
				laserSight = null;
			}
			//LaserOff();

			return;
		}

		if ( (TimeLastAttack * 4) >= AttackTime && !lockedOnTarget )
		{
			lockedOnTarget = true;
			//laserSight = Particles.Create( "particles/sniper_beam.vpcf", Target.EyePosition );
			//laserSight.SetEntityAttachment( 0, this, "muzzle" );
			//LaserOn();
		}


	}

	[ClientRpc]
	protected void LaserOn()
	{
		Host.AssertClient();

		laserSight = Particles.Create( "particles/sniper_beam.vpcf", Target.Position );
	}

	[ClientRpc]
	protected void LaserTarget(BaseNPC target)
	{
		laserSight.SetEntityAttachment( 0, this, "muzzle" );
		
		Log.Info( laserSight );
	}

	[ClientRpc]
	protected void LaserOff()
	{
		Host.AssertClient();

		if ( laserSight != null )
		{
			laserSight.Destroy( true );
			laserSight = null;
		}
	}

	public override void Attack( BaseNPC target )
	{
		LaserOff();

		lockedOnTarget = false;

		base.Attack( target );
	}
}
