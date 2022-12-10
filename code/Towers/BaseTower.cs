using System;
using System.Collections.Generic;
using Sandbox;

public partial class BaseTower : AnimatedEntity
{
	//Basic information
	public virtual string TowerName { get; set; } = "BASE TOWER";
	public virtual string TowerDesc { get; set; } = "BASE TOWER FOR ALL TOWERS";
	public virtual string TowerModel { get; set; } = "";

	//Requirements
	public virtual int UnlockLevel => 0;
	public virtual BaseTower RequiredTowers => null;

	//Levelling 
	public virtual string[] TowerLevelDesc => new[] 
	{ 
		"LEVEL 1",
		"LEVEL 2",
		"LEVEL 3",
		"LEVEL 4"
	};

	public virtual int[] TowerLevelCosts => new[]
	{
		1,
		2,
		3,
		4
	};

	//Each upgrade reduces attack time, increase damage and range
	//TODO: figure out variant paths
	public virtual List<(float AttTime, float AttDMG, int NewRange)> Upgrades => new();

	public virtual int TowerMaxLevel => 2;

	public int TowerLevel = 1;

	//Costs
	public virtual int TowerCost { get; set; } = 1;

	//Attacking + Deployment
	public virtual float DeploymentTime { get; set; } = 1.0f;
	public virtual float AttackTime { get; set; } = 1.0f;
	public virtual float AttackDamage { get; set; } = 1.0f;

	//How far it can see
	public virtual int RangeDistance { get; set; } = 10;
	public virtual string AttackSound => "";
	public bool IsPreviewing { get; set; } = true;

	[Net]
	public string NetName { get; set; }

	[Net]
	public string NetDesc { get; set; }

	[Net]
	public int NetCost { get; set; }

	public virtual string[] TowerUpgradeDesc => new[]
	{
		"LEVEL 1",
		"LEVEL 2",
		"LEVEL 3",
		"LEVEL 4"
	};

	[Net]
	public string NetUpgradeDesc { get; set; }

	[Net]
	public string NetStats { get; set; }

	public virtual bool CounterStealth { get; set; } = false;
	public virtual bool CounterAirborne { get; set; } = false;

	public TimeSince TimeSinceDeployed;
	public TimeSince TimeLastAttack;
	public TimeSince TimeLastUpg;

	public BaseNPC Target;

	public bool HasEnhanced;

	float _scanRot;

	float baseAttSpeed;
	int baseRange;
	float baseDamage;

	public override void Spawn()
	{
		SetModel( TowerModel );
		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );

		_scanRot = 0;

		TimeSinceDeployed = 0;

		if ( IsPreviewing )
			NetCost = TowerCost;
		else
		{
			baseDamage = AttackDamage;
			baseAttSpeed = AttackTime;
			baseRange = RangeDistance;
			HasEnhanced = false;

			NetCost = TowerLevelCosts[TowerLevel - 1];
			PlayDeployAnimRPC( To.Everyone );
		}

		NetName = TowerName;
		NetDesc = TowerDesc;
		NetUpgradeDesc = TowerUpgradeDesc[TowerLevel - 1];
		NetStats = $"DPS {MathF.Round( AttackDamage * AttackTime, 2 )} | Range {RangeDistance}";

		Tags.Add( "tower" );
	}

	public void PreviewSpawn()
	{
		SetModel( TowerModel );
		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );

		NetCost = TowerCost;
		NetName = TowerName;
		NetDesc = TowerDesc;

		Tags.Add( "tower" );
	}

	[ClientRpc]
	public void PlayDeployAnimRPC()
	{
		SetAnimParameter( "b_deploy", true );
	}

	[ClientRpc]
	public void PlayUpgradeAnimRPC()
	{
		SetAnimParameter( "b_upgrade", true );
	}

	public bool CanUpgrade()
	{
		if ( TowerLevel >= TowerMaxLevel )
			return false;

		if ( TimeLastUpg <= 1.5 )
			return false;

		if ( TimeSinceDeployed <= DeploymentTime )
			return false;

		if ( (Owner as CDPawn).GetCash() < TowerLevelCosts[TowerLevel - 1] )
			return false;

		return true;
	}

	public virtual void SellTower()
	{
		if ( !Game.IsServer )
			return;

		if(TowerLevel == 1)
			(Owner as CDPawn).AddCash( TowerCost / 2 );
		else
			(Owner as CDPawn).AddCash( TowerLevelCosts[TowerLevel - 1] / 2 );

		Delete();
	}

	public void ResetAndRestoreStats()
	{
		AttackTime = baseAttSpeed;
		AttackDamage = baseDamage;
		RangeDistance = baseRange;

		for ( int i = 1; i <= TowerLevel-1; i++ )
		{
			AttackTime += Upgrades[i-1].AttTime;
			AttackDamage += Upgrades[i-1].AttDMG;
			RangeDistance += Upgrades[i-1].NewRange;
		}

		NetUpgradeDesc = TowerUpgradeDesc[TowerLevel - 1];
		NetStats = $"DPS {MathF.Round( AttackDamage * AttackTime, 2 )} | Range {RangeDistance}";
	}

	public virtual void UpgradeTower()
	{
		if ( !CanUpgrade() )
			return;

		if ( Upgrades.Count <= TowerLevel - 1)
		{
			Log.Error( "Theres currently no upgrades for the next level" );
			return;
		}

		HasEnhanced = false;
		TimeLastUpg = 0;

		PlayUpgradeAnimRPC(To.Everyone);

		(Owner as CDPawn).TakeCash( TowerLevelCosts[TowerLevel - 1] );

		AttackTime += Upgrades[TowerLevel - 1].AttTime;
		AttackDamage += Upgrades[TowerLevel - 1].AttDMG;
		RangeDistance += Upgrades[TowerLevel - 1].NewRange;

		TowerLevel++;
		
		NetDesc = TowerLevelDesc[TowerLevel - 1];
		NetCost = TowerLevelCosts[TowerLevel - 1];
		NetUpgradeDesc = TowerUpgradeDesc[TowerLevel - 1];

		NetStats = $"DPS {MathF.Round(AttackDamage * AttackTime, 2)} | Range {RangeDistance}";
	}

	//Determine if the tower can attack this npc special type
	private bool CanTargetEnemy(BaseNPC npc)
	{
		//Target is cloaked
		if ( npc.AssetFile.NPCType == BaseNPCAsset.SpecialType.Hidden && !CounterStealth )
			return false;

		//Target is in the air
		if ( npc.AssetFile.NPCType == BaseNPCAsset.SpecialType.Airborne && !CounterAirborne )
			return false;

		return true;
	}

	//Scans for enemies
	public BaseNPC ScanForEnemy()
	{
		_scanRot++;

		var tr = Trace.Ray( Position + Vector3.Up * 5, Position + Rotation.FromYaw( _scanRot * 3.25f ).Forward * RangeDistance + Vector3.Up * 5 )
			.Ignore( this )
			.UseHitboxes()
			.WithoutTags( "cdplayer", "tower" )
			.Run();

		var tr2 = Trace.Ray( Position + Vector3.Up * 5, Position + Rotation.FromYaw( -_scanRot * 3.25f ).Forward * RangeDistance + Vector3.Up * 5 )
			.Ignore( this )
			.UseHitboxes()
			.WithoutTags( "cdplayer", "tower" )
			.Run();

		if ( CDGame.Instance.Debug && CDGame.Instance.DebugMode is CDGame.DebugEnum.Tower or CDGame.DebugEnum.All )
		{
			DebugOverlay.Line( tr.StartPosition, tr.EndPosition );
			DebugOverlay.Line( tr2.StartPosition, tr2.EndPosition );
		}

		if ( tr.Entity is BaseNPC npc )
		{
			if ( CDGame.StaticCompetitive && !npc.CastleTarget.TeamCastle.ToString().Contains( (Owner as CDPawn).CurTeam.ToString() ) )
				return null;

			if ( !CanTargetEnemy(npc) )
				return null;

			return npc;
		}

		if ( tr2.Entity is BaseNPC npc2 )
		{
			if ( CDGame.StaticCompetitive && !npc2.CastleTarget.TeamCastle.ToString().Contains( (Owner as CDPawn).CurTeam.ToString() ) )
				return null;

			if ( !CanTargetEnemy( npc2 ) )
				return null;

			return npc2;
		}

		return null;
	}

	public List<BaseNPC> ScanForEnemies()
	{
		var npcList = new List<BaseNPC>();

		var ents = FindInSphere( Position, RangeDistance );

		foreach ( var ent in ents )
		{
			if ( ent is BaseNPC npc )
			{
				if ( !CanTargetEnemy( npc ) )
					continue;

				if ( CDGame.StaticCompetitive && !npc.CastleTarget.TeamCastle.ToString().Contains( (Owner as CDPawn).CurTeam.ToString() ) )
					return null;

				var wallTr = Trace.Ray( Position + Vector3.Up * 15, npc.Position + Vector3.Up * 10)
				.Ignore( this )
				.Run();

				if ( wallTr.Entity is WorldEntity )
					continue;

				npcList.Add( npc );
			}
		}

		if ( CDGame.Instance.Debug && CDGame.Instance.DebugMode is CDGame.DebugEnum.Tower or CDGame.DebugEnum.All )
		{
			DebugOverlay.Sphere( Position, RangeDistance, Color.Yellow );

			foreach (var npc in npcList)
				DebugOverlay.Line( Position, npc.Position, Color.Yellow, AttackTime );
		}

		return npcList;
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

		FireEffects();

		TimeLastAttack = 0;
		var dmgInfo = new DamageInfo { Attacker = this, Damage = AttackDamage };

		(Owner as CDPawn).TotalDamage += (int)dmgInfo.Damage;

		target.TakeDamage( dmgInfo );
	}

	//Firing effects
	[ClientRpc]
	public virtual void FireEffects()
	{
		Game.AssertClient();
		PlaySound( AttackSound );
	}

	[ClientRpc]
	public virtual void FireEffectAtLocation(Vector3 pos)
	{
		Game.AssertClient();
		Sound.FromWorld( AttackSound, pos );
	}
}
