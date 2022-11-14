using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Sandbox;

public partial class TimeDisplacer : BaseSuperTower
{
	public override string TowerName => "Time Displacer";
	public override string TowerDesc => "A sort of time experiment able to teleport hostiles back to spawn";
	public override string TowerModel => "models/towers/timedisplacer.vmdl";
	public override int TowerCost => 650;
	public override float DeploymentTime => 6.5f; 
	public override string AttackSound => "timedisplacer_use";
	public override int RangeDistance => 64;

	public override void Spawn()
	{
		base.Spawn();

		NetAbility = "Ability: Reverts NPCs back to spawn, their health will not be restored";
	}

	public override void UseSuperAbility( TraceResult tr )
	{
		var ents = FindInSphere( tr.EndPosition, RangeDistance );

		var timeToRecover = 1.0f;

		foreach( var ent in ents )
		{
			if (ent is BaseNPC npc)
			{
				if( npc.IsBoss )
				{
					if ( npc.LastNode.IsNearlyZero() )
					{
						npc.Position = All.OfType<NPCSpawner>().FirstOrDefault().Position;
						npc.Steer.Target = All.OfType<NPCPath>().FirstOrDefault( x => x.StartNode ).Position;
					}
					else
						npc.Position = npc.LastNode;
				} 
				else
				{
					npc.Position = All.OfType<NPCSpawner>().FirstOrDefault().Position;
					npc.Steer.Target = All.OfType<NPCPath>().FirstOrDefault( x => x.StartNode ).Position;
				}

				npc.TimeUntilSpecialRecover = timeToRecover;
				timeToRecover += 1.5f;
			}
		}

		base.UseSuperAbility(tr);
	}
}

public partial class NukeSilo : BaseSuperTower
{
	public override string TowerName => "Nuclear Silo";
	public override string TowerDesc => "A weapon of mass destruction leaving nothing but a crater";
	public override string TowerModel => "models/towers/dummytower.vmdl";
	public override int TowerCost => 4500;
	public override float DeploymentTime => 6.5f;
	public override string AttackSound => "timedisplacer_launch";
	public override int RangeDistance => 256;

	bool isUsed;
	TimeSince timeNuked;
	bool emitRadiation;
	TraceResult _tr;

	public override void Spawn()
	{
		base.Spawn();
		emitRadiation = false;
		NetAbility = "Ability: Deals massive damage to targets within range of the blast zone and leaves radiation behind temporary";
	}

	[Event.Tick.Server]
	public override void SimulateTower()
	{
		base.SimulateTower();

		if(emitRadiation)
			DoRadiationEffects();
	}

	public override void UseSuperAbility( TraceResult tr )
	{
		if ( isUsed )
			return;

		_tr = tr;
		( Owner as CDPawn).CurSuperTower = null;

		isUsed = true;
		Sound.FromScreen(To.Everyone, "nuclearsilo_alert");

		_ = DoNukeActions();
	}

	public async Task DoNukeActions()
	{
		await Task.DelaySeconds(5.0f);

		Sound.FromWorld( "nuclearsilo_hit", _tr.EndPosition);
		
		await Task.DelaySeconds( 1.5f );
		
		Sound.FromWorld( "nuclearsilo_explode", _tr.EndPosition );
		DoExplosion();

		await Task.DelaySeconds( 7.5f );
		Despawn();
	}

	public void DoExplosion()
	{
		emitRadiation = true;

		foreach ( var ent in FindInSphere( _tr.EndPosition, RangeDistance) )
		{
			if ( ent is BaseNPC npc )
			{
				if(CDGame.StaticCompetitive)
				{
					var owner = Owner as CDPawn;

					if ( npc.CastleTarget.TeamCastle == CastleEntity.CastleTeam.Blue && owner.CurTeam != CDPawn.TeamEnum.Blue )
						continue;

					if ( npc.CastleTarget.TeamCastle == CastleEntity.CastleTeam.Red && owner.CurTeam != CDPawn.TeamEnum.Red )
						continue;
				}

				npc.TakeDamage( new DamageInfo()
				{
					Damage = 500.0f,
					Attacker = this
				} );
			}
		}
	}

	public void DoRadiationEffects()
	{
		if( timeNuked > 1.5f )
		{
			foreach ( var ent in FindInSphere( _tr.EndPosition, RangeDistance / 2) )
			{
				if(ent is BaseNPC npc)
				{
					npc.TakeDamage( new DamageInfo()
					{
						Damage = 50.0f,
						Attacker = this
					} );
				}
			}
			timeNuked = 0;
		}
	}
}
