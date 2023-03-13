using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Sandbox;
using Sandbox.Component;

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
	[Net] public bool IsPreviewing { get; set; } = true;

	[Net]
	public string NetName { get; set; }

	[Net]
	public string NetDesc { get; set; }

	[Net]
	public int NetCost { get; set; }

	[Net]
	public string NetUpgradeDesc { get; set; }

	[Net]
	public string NetStats { get; set; }

	[Net]
	public int NetRange { get; set; }

	//Counters for specific NPC types
	public virtual bool CounterStealth { get; set; } = false;
	public virtual bool CounterAirborne { get; set; } = false;

	//Timesince for deployment, attack and last upgrade
	public TimeSince TimeSinceDeployed;
	public TimeSince TimeLastAttack;
	public TimeSince TimeLastUpg;

	public BaseNPC Target;

	public bool HasEnhanced;

	//Base values for restoring, will be set on spawn
	float baseAttSpeed;
	int baseRange;
	float baseDamage;

	public virtual float ZEyeScale => 20.0f; 

	public enum PriorityEnum
	{
		None,
		LowestHP,
		HighHP,
		//FirstInLine,
	}

	[Net] public PriorityEnum TargetPriority { get; protected set; }

	public static int StaticUnlockLevel { get; set; }

	public static int GetUnlockLevel()
	{
		return StaticUnlockLevel;
	}

	public static int GetUnlockLevel(string towerName)
	{
		var tower = TypeLibrary.Create<BaseTower>( towerName );

		int lvl = tower.UnlockLevel;
		tower.Delete();

		return lvl;
	}

	public override void Spawn()
	{
		StaticUnlockLevel = UnlockLevel;

		SetModel( TowerModel );
		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );

		TimeSinceDeployed = 0;

		//If we're previewing it, just show cost
		if ( IsPreviewing )
			NetCost = TowerCost;
		else
		{
			//If not, set base values and other values
			baseDamage = AttackDamage;
			baseAttSpeed = AttackTime;
			baseRange = RangeDistance;
			HasEnhanced = false;

			NetCost = TowerLevelCosts[TowerLevel - 1];

			TargetPriority = PriorityEnum.None;
		}

		NetName = TowerName;
		NetDesc = TowerDesc;
		NetRange = RangeDistance;

		NetUpgradeDesc = SetUpDescription();
		NetStats = SetUpStatInfo();

		Tags.Add( "tower" );
	}

	public string SetUpStatInfo()
	{
		string newInfo = "";
		newInfo = $"Damage: {AttackDamage} | Delay: {MathF.Round( AttackTime, 2 )} | Range: {RangeDistance}";

		newInfo += $" | DPS: {MathF.Round( AttackDamage / AttackTime, 2 )}";

		return newInfo;
	}

	public string SetUpDescription()
	{
		string newDesc = "";

		if ( Upgrades[TowerLevel-1].AttTime != 0 )
			newDesc += "Speed: " + -Upgrades[TowerLevel - 1].AttTime;

		if ( Upgrades[TowerLevel-1].AttDMG != 0 )
			newDesc += " | Damage: " + Upgrades[TowerLevel - 1].AttDMG;

		if ( Upgrades[TowerLevel - 1].NewRange != 0 )
			newDesc += " | Range: " + Upgrades[TowerLevel - 1].NewRange;

		return newDesc;
	}
		
	public void PreviewSpawn()
	{
		SetModel( TowerModel );
		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );

		NetCost = TowerCost;
		NetName = TowerName;
		NetDesc = TowerDesc;
		NetStats = SetUpStatInfo();

		Tags.Add( "tower" );
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	[ClientRpc]
	public void DestroyComponents()
	{
		Components.RemoveAny<Glow>();
	}

	[ClientRpc]
	public void SimulateOwnership()
	{
		if ( !IsValid )
			return;
		
		var glow = Components.GetOrCreate<Glow>();
		glow.Width = 0.5f;
		glow.Color = Color.Cyan;
	}

	public void PlayDeployAnimRPC()
	{
		SetAnimParameter( "b_deploy", true );
	}

	public void PlayUpgradeSound()
	{
		Sound.FromEntity( "upgrade", this );
	}

	public bool CanUpgrade()
	{
		if ( !Game.IsServer )
			return false;

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

		DestroyComponents( To.Single( Owner ) );

		if (TowerLevel == 1)
			(Owner as CDPawn).AddCash( TowerCost / 2 );
		else if (TowerLevel < TowerMaxLevel)
			(Owner as CDPawn).AddCash( TowerLevelCosts[TowerLevel-1] / 2 );
		else if (TowerLevel == TowerMaxLevel)
			(Owner as CDPawn).AddCash( TowerLevelCosts[TowerLevel-2] / 2 );

		Delete();
	}

	//Resets stats back to base values
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

		NetRange = RangeDistance;
		NetUpgradeDesc = SetUpDescription();
		NetStats = SetUpStatInfo();
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

		PlayUpgradeSound();

		(Owner as CDPawn).TakeCash( TowerLevelCosts[TowerLevel - 1] );

		AttackTime += Upgrades[TowerLevel - 1].AttTime;
		AttackDamage += Upgrades[TowerLevel - 1].AttDMG;
		RangeDistance += Upgrades[TowerLevel - 1].NewRange;

		TowerLevel++;
		
		NetDesc = TowerLevelDesc[TowerLevel - 1];
		NetCost = TowerLevelCosts[TowerLevel - 1];
		NetRange = RangeDistance;
		NetUpgradeDesc = SetUpDescription();

		NetStats = SetUpStatInfo();
	}

	//Determine if the tower can attack this npc special type
	private bool CanTargetEnemy(BaseNPC npc)
	{
		//Target is behind a wall
		var wallTr = Trace.Ray( Position + Vector3.Up * ZEyeScale, npc.Position + Vector3.Up * ZEyeScale )
				.Ignore( this )
				.WorldOnly()
				.Run();

		if ( wallTr.Entity is WorldEntity )
			return false;

		if ( Target == npc )
			return false;

		//Target is cloaked
		if ( npc.AssetFile.NPCType == BaseNPCAsset.SpecialType.Hidden && !CounterStealth )
			return false;

		//Target is in the air
		if ( npc.AssetFile.NPCType == BaseNPCAsset.SpecialType.Airborne && !CounterAirborne )
			return false;

		return true;
	}

	int curOrder = -1;

	int enumIndex = -1;

	public void SetNextPriority()
	{
		enumIndex++;

		//If we're over the enums count, set index back to 0
		if ( enumIndex > Enum.GetValues( typeof( PriorityEnum ) ).Cast<PriorityEnum>().Count() - 1 )
			enumIndex = 0;

		TargetPriority = (PriorityEnum)enumIndex;
	}

	public bool ShouldPrioritizeTarget(BaseNPC newTarget)
	{
		if ( newTarget == null ) return false;

		//If we are targetting lowest health npcs
		if ( TargetPriority == PriorityEnum.LowestHP )
		{
			if ( newTarget.Health < Target.Health )
				return true;
		}

		//If we are targetting highest health npcs
		if(TargetPriority == PriorityEnum.HighHP)
		{
			if ( newTarget.Health > Target.Health )
				return true;
		}

		//If we are targetting first in line --BROKEN
		/*if ( TargetPriority == PriorityEnum.FirstInLine )
		{
			if ( curOrder == -1 )
			{
				curOrder = newTarget.Order;
				return true;
			}

			if ( newTarget.Order == curOrder ) return true;
		}*/

		return false;
	}

	//Scans for enemies
	public virtual BaseNPC ScanForEnemy(BaseNPC ignoreNPC = null)
	{
		var ents = FindInSphere( Position, RangeDistance );

		foreach ( var ent in ents )
		{
			if ( ent is BaseNPC npc )
			{
				if ( npc == ignoreNPC )
					continue;

				if ( !CanTargetEnemy( npc ) )
					continue;

				if ( CDGame.StaticCompetitive && !npc.CastleTarget.TeamCastle.ToString().Contains( (Owner as CDPawn).CurTeam.ToString() ) )
					return null;

				return npc;
			}
		}

		return null;
	}

	public virtual List<BaseNPC> ScanForEnemies()
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

				npcList.Add( npc );
			}
		}

		if ( CDGame.CDDebug && CDGame.Instance.DebugMode is CDGame.DebugEnum.Tower or CDGame.DebugEnum.All )
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

		SimulateOwnership( To.Single( Owner ) );

		//Still deploying, wait until finished
		if ( TimeSinceDeployed < DeploymentTime )
			return;

		if ( Target == null )
			Target = ScanForEnemy();

		if( TargetPriority != PriorityEnum.None && Target != null )
		{
			var newNPC = ScanForEnemy(Target);

			if ( ShouldPrioritizeTarget( newNPC ) )
				Target = newNPC;
		}

		//If we have a target and is within range, attack it
		if (Target.IsValid() && Position.Distance(Target.Position) < RangeDistance)
		{
			//Trace check
			var towerTR = Trace.Ray( Position + Vector3.Up * ZEyeScale, Target.Position + Target.Model.Bounds.Center / 2 + Vector3.Up * ZEyeScale )
				.Ignore( this )
				.WithoutTags( "tower", "cdplayer" )
				.UseHitboxes(true)
				.Run();

			if ( CDGame.CDDebug &&
				(CDGame.Instance.DebugMode == CDGame.DebugEnum.Tower || CDGame.Instance.DebugMode == CDGame.DebugEnum.All) )
				DebugOverlay.Line( towerTR.StartPosition, towerTR.EndPosition );

			//A wall is blocking the towers sight to the target
			if ( towerTR.Entity is not BaseNPC )
			{
				Target = null;
				return;
			}

			if ( TimeLastAttack >= AttackTime )
				Attack( Target );
		}
		//Else we lost sight or the target died
		else
		{
			Target = null;
		}

		if ( All.OfType<BaseNPC>().Count() == 0 )
			curOrder = -1;
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

		if ( target.Health <= 0 )
			curOrder++;
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
