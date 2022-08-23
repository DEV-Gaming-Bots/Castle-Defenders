using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Sandbox;


public partial class BaseTower : AnimatedEntity
{
	//Basic information
	public virtual string TowerName => "BASE TOWER";
	public virtual string TowerDesc => "BASE TOWER FOR ALL TOWERS";
	public virtual string TowerModel => "";

	//Requirements
	public virtual int UnlockLevel => 0;
	public virtual BaseTower RequiredTowers => null;

	//Levelling 
	public virtual string[] TowerLevelDesc => new string[] 
	{ 
		"LEVEL 1",
		"LEVEL 2",
		"LEVEL 3",
		"LEVEL 4"
	};

	public virtual int[] TowerLevelCosts => new int[]
	{
		1,
		2,
		3,
		4
	};

	//Each upgrade reduces attack time, increase damage and range
	//TODO: figure out variant paths
	public virtual List<(float AttTime, float AttDMG, int NewRange)> Upgrades => new()
	{

	};

	public virtual int TowerMaxLevel => 2;

	public int TowerLevel = 1;

	//Costs
	public virtual int TowerCost => 1;

	//Attacking + Deployment
	public virtual float DeploymentTime => 1.0f;
	public virtual float AttackTime { get; set; } = 1.0f;
	public virtual float AttackDamage { get; set; } = 1.0f;

	//How far it can see
	public virtual int RangeDistance { get; set; } = 10;
	public virtual string AttackSound => "";

	[Net]
	public bool IsPreviewing { get; set; } = true;

	[Net]
	public string NetName { get; set; }

	[Net]
	public string NetDesc { get; set; }

	[Net]
	public int NetCost { get; set; }

	public TimeSince TimeSinceDeployed;
	public TimeSince TimeLastUpgrade;
	public TimeSince TimeLastAttack;

	public BaseNPC Target;

	public override void Spawn()
	{
		SetModel( TowerModel );
		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );

		if ( !IsPreviewing )
		{
			TimeSinceDeployed = 0;
			NetCost = TowerLevelCosts[TowerLevel - 1];
			PlayDeployAnimation();
		} 
		else
			NetCost = TowerCost;

		NetName = TowerName;
		NetDesc = TowerDesc;

		Tags.Add( "tower" );
	}
	public override void ClientSpawn()
	{
		base.ClientSpawn();
	}

	[Event.Hotload()]
	public void HotloadTowers()
	{
		
	}

	[ClientRpc]
	public void PlayDeployAnimation()
	{
		SetAnimParameter( "b_deploy", true );
	}

	public void PlayUpgradeAnimation()
	{
		SetAnimParameter( "b_upgrade", true );
	}

	public bool CanUpgrade()
	{
		if ( TowerLevel >= TowerMaxLevel )
			return false;

		if ( TimeSinceDeployed < DeploymentTime )
			return false;

		if ( TimeLastUpgrade < 4.0f )
			return false;

		if ( (Owner as CDPawn).GetCash() < TowerLevelCosts[TowerLevel - 1] )
			return false;

		return true;
	}

	public void SellTower()
	{
		if ( !IsServer )
			return;

		if(TowerLevel == 1)
			(Owner as CDPawn).AddCash( TowerCost / 2 );
		else
			(Owner as CDPawn).AddCash( TowerLevelCosts[TowerLevel - 1] / 2 );

		Delete();
	}

	public virtual void UpgradeTower()
	{
		PlayUpgradeAnimation();

		if(IsServer)
		{
			if ( Upgrades.Count <= TowerLevel - 1)
			{
				Log.Error( "Theres currently no upgrades for the next level" );
				return;
			}

			(Owner as CDPawn).TakeCash( TowerLevelCosts[TowerLevel - 1] );

			TowerLevel++;

			AttackTime += Upgrades[TowerLevel - 1].AttTime;
			AttackDamage += Upgrades[TowerLevel - 1].AttDMG;
			RangeDistance += Upgrades[TowerLevel - 1].NewRange;

			NetDesc = TowerLevelDesc[TowerLevel - 1];
			NetCost = TowerLevelCosts[TowerLevel - 1];

			TimeLastUpgrade = 0;
		}

	}

	//Scans for enemies
	public BaseNPC ScanForEnemy()
	{
		for ( int i = 1; i <= 21; i++ )
		{
			var tr = Trace.Ray( Position + Vector3.Up * 5, Position + Rotation.FromYaw( i * 17 ).Forward * RangeDistance + Vector3.Up * 5 )
				.Ignore( this )
				.UseHitboxes( true )
				.WithoutTags( "cdplayer", "tower" )
				.Run();

			if ( CDGame.Instance.Debug && (CDGame.Instance.DebugMode == CDGame.DebugEnum.Tower || CDGame.Instance.DebugMode == CDGame.DebugEnum.All) )
				DebugOverlay.Line( tr.StartPosition, tr.EndPosition );

			if ( tr.Entity is BaseNPC npc )
				return npc;
		}

		return null;
	}

	public List<BaseNPC> ScanForEnemies()
	{
		List<BaseNPC> npclist = new List<BaseNPC>();

		for ( int i = 1; i <= 45; i++ )
		{
			var tr = Trace.Ray( Position + Vector3.Up * 5, Position + Rotation.FromYaw( i * 8 ).Forward * RangeDistance + Vector3.Up * 5 )
				.Ignore( this )
				.WithoutTags( "cdplayer", "tower" )
				.UseHitboxes( true )
				.Run();

			if ( CDGame.Instance.Debug && (CDGame.Instance.DebugMode == CDGame.DebugEnum.Tower || CDGame.Instance.DebugMode == CDGame.DebugEnum.All) )
				DebugOverlay.Line( tr.StartPosition, tr.EndPosition );

			if ( tr.Entity is BaseNPC npc )
				npclist.Add( npc );
		}

		return npclist;
	}

	[Event.Tick.Server]
	public virtual void SimulateTower()
	{
		//This is a preview tower, do nothing else
		if ( IsPreviewing )
			return;

		//Still deploying, wait until finished
		if ( TimeSinceDeployed < DeploymentTime )
			return;

		//Tower doesn't have a target, find one
		if(Target == null)
			Target = ScanForEnemy();

		//If we have a target and is within range, attack it
		if (Target.IsValid() && Position.Distance(Target.Position) < RangeDistance)
		{
			//Trace check
			var towerTR = Trace.Ray( Position + Vector3.Up * 10, Target.Position + Target.Model.Bounds.Center / 2 )
				.Ignore( this )
				.WithoutTags( "tower", "cdplayer" )
				.UseHitboxes(true)
				.Run();

			if ( CDGame.Instance.Debug &&
				(CDGame.Instance.DebugMode == CDGame.DebugEnum.Tower || CDGame.Instance.DebugMode == CDGame.DebugEnum.All) )
				DebugOverlay.Line( towerTR.StartPosition, towerTR.EndPosition );

			//A wall is blocking the towers sight to the target
			if ( towerTR.Entity is not BaseNPC )
			{
				Target = null;
				return;
			}

			if( TimeLastAttack >= AttackTime )
				Attack( Target );
		}
		//Else we lost sight or the target died
		else
		{
			Target = null;
		}
	}

	//Attack the target NPC
	public virtual void Attack(BaseNPC target)
	{
		if ( IsPreviewing )
			return;

		PlaySound( AttackSound );
		FireEffects();

		TimeLastAttack = 0;
		DamageInfo dmgInfo = new DamageInfo();
		dmgInfo.Attacker = this;
		dmgInfo.Damage = AttackDamage;

		target.TakeDamage( dmgInfo );
	}

	//Firing effects
	[ClientRpc]
	public virtual void FireEffects()
	{
		Host.AssertClient();
	}
}
