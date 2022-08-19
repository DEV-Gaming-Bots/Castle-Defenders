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
	public override string TowerDesc => "A pistol tower";

	//Temporary until we get a pistol model
	public override string TowerModel => "models/towers/trickster_tower.vmdl";
	public override int UnlockLevel => 0;
	public override BaseTower RequiredTowers => null;
	public override string[] TowerLevelDesc => new string[]
	{
		"LEVEL 1 TEMPLATE DESCRIPTION",
		"LEVEL 2 TEMPLATE DESCRIPTION",
		"LEVEL 3 TEMPLATE DESCRIPTION",
		"LEVEL 4 TEMPLATE DESCRIPTION",
		"LEVEL 5 TEMPLATE DESCRIPTION"
	};

	public override int TowerMaxLevel => 5;
	public override int TowerCost => 1;
	public override float DeploymentTime => 0.35f;
	public override float AttackTime => 1.0f;
	public override int AttackDamage => 5;
	public override int RangeDistance => 175;
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
	public override string[] TowerLevelDesc => new string[]
	{
		"LEVEL 1 TEMPLATE DESCRIPTION",
		"LEVEL 2 TEMPLATE DESCRIPTION",
		"LEVEL 3 TEMPLATE DESCRIPTION",
		"LEVEL 4 TEMPLATE DESCRIPTION",
		"LEVEL 5 TEMPLATE DESCRIPTION"
	};

	public override int TowerMaxLevel => 5;
	public override int TowerCost => 1;
	public override float DeploymentTime => 0.45f;
	public override float AttackTime => 3.5f;
	public override int AttackDamage => 5;
	public override int RangeDistance => 175;
	public override string AttackSound => "sniper_fire";

	bool lockedOnTarget;
	Particles laserSight;

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
			timeLastAttack = 0;
			lockedOnTarget = false;
			
			if ( laserSight != null )
			{
				laserSight.Destroy( true );
				laserSight = null;
			}
			//LaserOff();

			return;
		}

		if ( (timeLastAttack * 4) >= AttackTime && !lockedOnTarget )
		{
			lockedOnTarget = true;
			laserSight = Particles.Create( "particles/sniper_beam.vpcf", Target.EyePosition );
			laserSight.SetEntityAttachment( 0, this, "muzzle" );
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
