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

	BaseNPC target;

	public override void Spawn()
	{
		SetModel( TowerModel );
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
		SetAnimParameter( "deploy", true );
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

		//Once deployment finishes, set the anim bool to false
		//NOTE: tried to auto reset in animgraph, didn't work
		if(timeDeployed >= DeploymentTime && GetAnimParameterBool("deploy") )
			SetAnimParameter( "deploy", false );

		//Tower doesn't have a target, find one
		if(target == null)
		{
			var entities = FindInSphere( Position + Vector3.Up * 10, RangeDistance );

			foreach ( var ent in entities )
			{
				if( ent is BaseNPC npc )
					target = npc;
			}
		}

		//If we have a target and is within range, attack it
		if (target.IsValid() && Position.Distance(target.Position) <= RangeDistance)
		{
			//Trace check
			var towerTR = Trace.Ray( Position + Vector3.Up * 10, target.EyePosition )
				.Ignore( this )
				.Run();

			//A wall is blocking the towers sight to the target
			if ( towerTR.Entity is not BaseNPC )
			{
				target = null;
				return;
			}

			if( timeLastAttack >= AttackTime )
				Attack( target );
		}
		//Else we lost sight or the target died
		else
		{
			target = null;
		}
	}

	//Attack the target NPC
	public virtual void Attack(BaseNPC target)
	{
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

		PlaySound( AttackSound );
	}
}
