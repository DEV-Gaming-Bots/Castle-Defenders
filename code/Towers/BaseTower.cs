using System;
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

	public virtual int TowerMaxLevel => 2;

	public int TowerLevel = 1;

	//Costs
	public virtual int TowerCost => 1;

	//Attacking + Deployment
	public virtual float DeploymentTime => 1.0f;
	public virtual float AttackTime => 1.0f;
	public virtual int AttackDamage => 1;

	//How far it can see
	public virtual int RangeDistance => 10;
	public virtual string AttackSound => "";

	public bool IsPreviewing;

	TimeSince timeDeployed;
	public TimeSince timeLastAttack;

	public BaseNPC Target;

	public override void Spawn()
	{
		SetModel( TowerModel );
		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );

		if ( !IsPreviewing )
			SetAnimParameter( "deploy", false );

		Tags.Add( "tower" );
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();
		IsPreviewing = true;
	}

	public virtual void Deploy()
	{
		Spawn();
		timeDeployed = 0;
	}

	//Scans for enemies
	public BaseNPC ScanForEnemy()
	{
		for ( int i = 1; i <= 360; i++ )
		{
			var tr = Trace.Ray( Position + Vector3.Up * 5, Position + Rotation.FromYaw( i ).Forward * RangeDistance + Vector3.Up * 5 )
				.Ignore( this )
				.UseHitboxes( true )
				.Run();

			if ( CDGame.Instance.DebugMode )
				DebugOverlay.Line( tr.StartPosition, tr.EndPosition );

			if ( tr.Entity is BaseNPC npc )
				return npc;
		}

		return null;
	}

	[Event.Tick.Server]
	public virtual void SimulateTower()
	{
		//This is a preview tower, do nothing else
		if ( IsPreviewing )
			return;

		//Still deploying, wait until finished
		if ( timeDeployed < DeploymentTime )
			return;

		//Tower doesn't have a target, find one
		if(Target == null)
			Target = ScanForEnemy();

		//If we have a target and is within range, attack it
		if (Target.IsValid() && Position.Distance(Target.Position) < RangeDistance)
		{
			//Trace check
			var towerTR = Trace.Ray( Position + Vector3.Up * 5, Target.Position + Vector3.Up * 5 )
				.Ignore( this )
				.UseHitboxes(true)
				.Run();

			if ( CDGame.Instance.DebugMode )
				DebugOverlay.Line( towerTR.StartPosition, towerTR.EndPosition );

			//A wall is blocking the towers sight to the target
			if ( towerTR.Entity is not BaseNPC )
			{
				Target = null;
				return;
			}

			if( timeLastAttack >= AttackTime )
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

		timeLastAttack = 0;
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
