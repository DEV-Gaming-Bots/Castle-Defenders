using System;
using System.Linq;
using System.Collections.Generic;
using Sandbox;
using System.IO;

public partial class BaseNPC : AnimatedEntity
{
	//Basics
	public virtual string NPCName => "Default NPC";
	public virtual string BaseModel => "models/citizen/citizen.vmdl";
	public virtual float BaseHealth => 1;
	public virtual float BaseSpeed { get; set; } = 1;
	public virtual int[] MinMaxCashReward => new int[] { 1, 2 };
	public virtual int[] MinMaxEXPReward => new int[] { 1, 2 };
	public virtual float NPCScale => 1;
	public virtual float Damage => 1;

	public enum SpecialType
	{
		Standard,
		Armoured,
		Hidden,
		Disruptor,
		Splitter,
	}

	//Special Types of NPCs:
	//Standard - No special trait
	//Armoured - Takes more hits before armor breaks
	//Hidden - Invisible to towers without special ability
	//Disrupter - Disrupts nearby towers with exceptions
	//Splitter - Splits into multiple weaker versions of the NPC upon death
	public virtual SpecialType NPCType => SpecialType.Standard;
	public virtual float ArmourStrength => 0;
	public virtual int SplitAmount => 0;

	[ConVar.Replicated]
	public static bool td2_npc_drawoverlay { get; set; }

	public int CashReward;
	public int ExpReward;
	public int PathTarget;

	public NPCPathSteer Steer;

	Vector3 InputVelocity;
	Vector3 LookDir;

	CastleEntity castleTarget;

	public TimeUntil TimeUntilSpecialRecover;

	public int GetDifficulty()
	{
		switch ( CDGame.Instance.Difficulty )
		{
			case CDGame.DiffEnum.Easy:
				return 1;

			case CDGame.DiffEnum.Medium:
				return 2;

			case CDGame.DiffEnum.Hard:
				return 3;

			case CDGame.DiffEnum.Extreme:
				return 4;
		}

		return 0;
	}

	public override void Spawn()
	{ 
		SetModel( BaseModel );

		PathTarget = 1;

		Position = All.OfType<NPCSpawner>().First().Position;

		Scale = NPCScale;
		Health = BaseHealth * GetDifficulty();

		CashReward = Rand.Int( MinMaxCashReward[0], MinMaxCashReward[1] ) * GetDifficulty() / 2;
		ExpReward = Rand.Int( MinMaxEXPReward[0], MinMaxEXPReward[1] ) * GetDifficulty();

		Tags.Add( "npc" );

		SetupPhysicsFromOBB( PhysicsMotionType.Keyframed, Model.Bounds.Mins, Model.Bounds.Maxs );
		EnableTraceAndQueries = true;
		EnableHitboxes = false;

		var spawnerpoint = All.OfType<NPCSpawner>().ToList();
		var blueSide = spawnerpoint.FirstOrDefault( x => x.AttackTeamSide == NPCSpawner.TeamEnum.Blue );
		var redSide = spawnerpoint.FirstOrDefault( x => x.AttackTeamSide == NPCSpawner.TeamEnum.Red );

		if ( blueSide != null)
			castleTarget = blueSide.FindCastle();

		if ( redSide != null)
			castleTarget = redSide.FindCastle();

		Steer = new NPCPathSteer();
		Steer.Target = All.OfType<NPCPath>().Where( x => x.StartNode ).FirstOrDefault().Position;
	}

	//When the NPC reaches the castle, despawn
	public void Despawn()
	{
		EnableDrawing = false;
		Delete();
	}

	public void FollowPath()
	{
		if ( CDGame.Instance.GameStatus == CDGame.GameEnum.Post )
			Despawn();

		if ( castleTarget.IsValid() && Position.Distance( castleTarget.Position ) <= 25.0f )
		{
			DamageInfo dmgInfo = new DamageInfo();
			dmgInfo.Damage = Damage * GetDifficulty();

			castleTarget.DamageCastle( dmgInfo.Damage );
			Despawn();
			return;
		}

		if ( Steer.Target.Distance( Position ) <= 25.0f )
		{
			foreach ( var path in All.OfType<NPCPath>() )
			{
				if ( path.Position.Distance( Position ) <= 25.0f )
				{
					if ( path.FindSplitPath() != null )
					{
						switch ( Rand.Int( 1, 2 ) )
						{
							case 1:
								Steer.Target = path.FindNormalPath().Position;
								break;
							case 2:
								Steer.Target = path.FindSplitPath().Position;
								break;
						}

						break;
					}

					if( path.FindNormalPath() != null )
					{
						Steer.Target = path.FindNormalPath().Position;
						break;
					}
				}
			}
		}
	}

	//Server ticking for NPC Navigation
	[Event.Tick.Server]
	public void Tick()
	{
		InputVelocity = 0;		

		if ( Steer != null || !IsValid )
		{
			FollowPath();

			Steer.Tick( Position );

			if ( !Steer.Output.Finished )
			{
				InputVelocity = Steer.Output.Direction.Normal;
				Velocity = Velocity.AddClamped( InputVelocity * Time.Delta * 500, BaseSpeed * 1.5f );
			}			
		}

		if ( TimeUntilSpecialRecover > 0.0f )
			return;

		Move( Time.Delta );

		var walkVelocity = Velocity.WithZ( 0 );
		if ( walkVelocity.Length > 0.5f )
		{
			var turnSpeed = walkVelocity.Length.LerpInverse( 0, 100, true );
			var targetRotation = Rotation.LookAt( walkVelocity.Normal, Vector3.Up );
			Rotation = Rotation.Lerp( Rotation, targetRotation, turnSpeed * Time.Delta * 20.0f );
		}

		var animHelper = new NPCAnimationHelper( this );

		LookDir = Vector3.Lerp( LookDir, InputVelocity.WithZ( 0 ) * 1000, Time.Delta * 100.0f );
		animHelper.WithLookAt( EyePosition + LookDir );
		animHelper.WithVelocity( Velocity );
		animHelper.WithWishVelocity( InputVelocity );
	}
	protected virtual void Move( float timeDelta )
	{
		var bbox = BBox.FromHeightAndRadius( 64, 4 );

		MoveHelper move = new( Position, Velocity );
		move.MaxStandableAngle = 50;
		move.Trace = move.Trace.Ignore( this ).Size( bbox );

		if ( !Velocity.IsNearlyZero( 0.001f ) )
		{
			move.TryUnstuck();
			move.TryMoveWithStep( timeDelta, 30 );
		}

		var tr = move.TraceDirection( Vector3.Down * 10.0f );

		if ( move.IsFloor( tr ) )
		{
			GroundEntity = tr.Entity;

			if ( !tr.StartedSolid )
			{
				move.Position = tr.EndPosition;
			}
			
			if ( InputVelocity.Length > 0 )
			{
				var movement = move.Velocity.Dot( InputVelocity.Normal );
				move.Velocity = move.Velocity - movement * InputVelocity.Normal;
				move.ApplyFriction( tr.Surface.Friction * 10.0f, timeDelta );
				move.Velocity += movement * InputVelocity.Normal;
			}
			else
			{
				move.ApplyFriction( tr.Surface.Friction * 10.0f, timeDelta );
			}
		}
		else
		{
			GroundEntity = null;
			move.Velocity += Vector3.Down * 900 * timeDelta;
		}

		Position = move.Position;
		Velocity = move.Velocity;
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		Rotation = Input.Rotation;
		EyeRotation = Rotation;

		Velocity += Input.Rotation * new Vector3( Input.Forward, Input.Left, Input.Up ) * BaseSpeed * 5 * Time.Delta;
		if ( Velocity.Length > BaseSpeed ) Velocity = Velocity.Normal * BaseSpeed;

		Velocity = Velocity.Approach( 0, Time.Delta * BaseSpeed * 3 );

		Position += Velocity * Time.Delta;

		EyePosition = Position;
	}

	public override void TakeDamage( DamageInfo info )
	{
		Health -= info.Damage;

		if ( Health <= 0 )
		{
			OnKilled();
		}
	}

	public void Split()
	{

	}

	public override void OnKilled()
	{
		if ( !IsServer )
			return;

		All.OfType<CDPawn>().ToList().ForEach( x =>
		{
			x.AddCash( CashReward );
			x.AddEXP( ExpReward );
		} );

		base.OnKilled();
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		Rotation = Input.Rotation;
		EyeRotation = Rotation;
		Position += Velocity * Time.Delta;
	}
}
