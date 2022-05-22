using System;
using System.Linq;
using System.Collections.Generic;
using Sandbox;

public partial class BaseNPC : AnimatedEntity
{
	//Basics
	public virtual string NPCName => "Default NPC";
	public virtual string BaseModel => "models/citizen/citizen.vmdl";
	public virtual int BaseHealth => 1;
	public virtual float BaseSpeed { get; set; } = 1;
	public virtual int[] MinMaxCashReward => new int[] {1, 2};
	int cashReward = 0;
	public virtual int[] MinMaxEXPReward => new int[] { 1, 2 };
	int expReward = 0;
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

	[ConVar.Replicated]
	public static bool td2_npc_drawoverlay { get; set; }

	Vector3 InputVelocity;
	Vector3 LookDir;

	NPCNavigation Path = new NPCNavigation();
	public NPCPathSteer Steer;

	CastleEntity castleTarget;
	int pathTarget;

	//Spawns the NPC
	public override void Spawn()
	{ 
		SetModel( BaseModel );

		pathTarget = 0;

		Scale = NPCScale;
		Health = BaseHealth;

		EnableHitboxes = true;

		cashReward = Rand.Int( MinMaxCashReward[0], MinMaxCashReward[1]);
		expReward = Rand.Int( MinMaxEXPReward[0], MinMaxEXPReward[1] );

		Tags.Add( "TD2_NPC" );

		SetupPhysicsFromModel(PhysicsMotionType.Keyframed);
		EnableTraceAndQueries = true;

		Steer = new NPCPathSteer();
		castleTarget = All.OfType<CastleEntity>().FirstOrDefault();
	}

	//When the NPC reaches the castle, despawn
	public void Despawn()
	{
		EnableDrawing = false;
		Delete();
	}

	public void FollowPath()
	{
		if(Position.Distance( castleTarget.Position ) <= 50 )
		{
			DamageInfo dmgInfo = new DamageInfo();
			dmgInfo.Damage = Damage;

			castleTarget.TakeDamage( dmgInfo );
			Despawn();
		}

		if ( Position.Distance( Steer.Target ) <= 20 )
			pathTarget++;
		
		foreach ( var pathway in All.OfType<NPCPath>())
		{
			if ( pathTarget == pathway.Path_Order )
				Steer.Target = pathway.Position;
		}
	}

	//Server ticking for NPC Navigation
	[Event.Tick.Server]
	public void Tick()
	{
		InputVelocity = 0;		

		if ( Steer != null || !IsValid )
		{
			Steer.Tick( Position );

			if ( !Steer.Output.Finished )
			{
				InputVelocity = Steer.Output.Direction.Normal;
				Velocity = Velocity.AddClamped( InputVelocity * Time.Delta * 500, BaseSpeed * 1.5f );
			}

			FollowPath();
			
		}

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

	public override void OnKilled()
	{
		base.OnKilled();

		foreach ( var player in Client.All.OfType<TD2Pawn>())
		{
			if ( player == null )
				continue;

			player.AddCash( cashReward );
			player.AddEXP( expReward );
		}
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		Rotation = Input.Rotation;
		EyeRotation = Rotation;
		Position += Velocity * Time.Delta;
	}
}
