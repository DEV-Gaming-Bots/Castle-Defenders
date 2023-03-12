using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public partial class StandardController : BaseNetworkable
{
	protected HashSet<string> Events { get; set; } = new();
	protected HashSet<string> Tags { get; set; } = new();
	public virtual bool EnableSprinting => true;
	public virtual float SprintSpeed { get; set; } = 320f;
	public virtual float WalkSpeed { get; set; } = 150f;
	public float Acceleration { get; set; } = 10f;
	public float AirAcceleration { get; set; } = 50f;
	public float FallSoundZ { get; set; } = -30f;
	public float GroundFriction { get; set; } = 4f;
	public float StopSpeed { get; set; } = 100f;
	public float Size { get; set; } = 20f;
	public float DistEpsilon { get; set; } = 0.03125f;
	public float GroundAngle { get; set; } = 46f;
	public float Bounce { get; set; } = 0f;
	public float MoveFriction { get; set; } = 1f;
	public float StepSize { get; set; } = 18f;
	public float MaxNonJumpVelocity { get; set; } = 140f;
	public float BodyGirth { get; set; } = 32f;
	public float BodyHeight { get; set; } = 72f;
	public float EyeHeight { get; set; } = 64f;
	public float Gravity { get; set; } = 800f;
	public float AirControl { get; set; } = 30f;
	public bool Swimming { get; set; }
	public bool AutoJump { get; set; } = false;

	protected float SurfaceFriction { get; set; }
	protected bool IsTouchingLadder { get; set; }
	protected Vector3 LadderNormal { get; set; }
	protected Vector3 TraceOffset { get; set; }
	protected Vector3 PreVelocity { get; set; }
	protected Vector3 Mins { get; set; }
	protected Vector3 Maxs { get; set; }
	public Vector3 WishVelocity { get; protected set; }
	public Vector3 GroundNormal { get; set; }

	public DuckController Duck { get; private set; }

	public bool IsServer => Sandbox.Game.IsServer;
	public bool IsClient => Sandbox.Game.IsClient;

	[Net] public CDPawn Owner { get; protected set; }

	private int StuckTries { get; set; } = 0;

	public StandardController()
	{
		Duck = new DuckController( this );
	}

	public StandardController( CDPawn newOwner ) : this()
	{
		Owner = newOwner;
	}

	public void ClearGroundEntity()
	{
		if ( Owner.GroundEntity == null )
			return;

		Owner.GroundEntity = null;
		GroundNormal = Vector3.Up;
		SurfaceFriction = 1f;
	}

	public TraceResult TraceBBox( Vector3 start, Vector3 end, Vector3 mins, Vector3 maxs, float liftFeet = 0.0f )
	{
		if ( liftFeet > 0 )
		{
			start += Vector3.Up * liftFeet;
			maxs = maxs.WithZ( maxs.z - liftFeet );
		}

		var tr = Trace.Ray( start + TraceOffset, end + TraceOffset )
			.Size( mins, maxs )
			.WithoutTags( "passOwners" )
			.WithAnyTags( "solid", "Ownerclip", "passbullets", "Owner", "playerclip" )
			.Ignore( Owner )
			.Run();

		tr.EndPosition -= TraceOffset;
		return tr;
	}

	public TraceResult TraceBBox( Vector3 start, Vector3 end, float liftFeet = 0f )
	{
		return TraceBBox( start, end, Mins, Maxs, liftFeet );
	}

	public BBox GetHull()
	{
		var girth = BodyGirth * 0.5f;
		var mins = new Vector3( -girth, -girth, 0 );
		var maxs = new Vector3( +girth, +girth, BodyHeight );
		return new BBox( mins, maxs );
	}

	public virtual void FrameSimulate()
	{
		Owner.EyeRotation = Owner.ViewAngles.ToRotation();
	}

	public virtual void Simulate()
	{
		Owner.EyeLocalPosition = Vector3.Up * (EyeHeight * Owner.Scale);
		UpdateBBox();

		Owner.EyeLocalPosition += TraceOffset;

		// If we're a bot, spin us around 180 degrees.
		if ( Owner.Client.IsBot )
			Owner.EyeRotation = Owner.ViewAngles.WithYaw( Owner.ViewAngles.yaw + 180f ).ToRotation();
		else
			Owner.EyeRotation = Owner.ViewAngles.ToRotation();

		if ( CheckStuckAndFix() )
			return;

		CheckLadder();
		Swimming = Owner.GetWaterLevel() > 0.6f;

		if ( !Swimming && !IsTouchingLadder )
		{
			Owner.Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;
			Owner.Velocity += new Vector3( 0, 0, Owner.BaseVelocity.z ) * Time.Delta;
			Owner.BaseVelocity = Owner.BaseVelocity.WithZ( 0 );
		}

		HandleJumping();

		var startOnGround = Owner.GroundEntity.IsValid();

		if ( startOnGround )
		{
			Owner.Velocity = Owner.Velocity.WithZ( 0 );

			if ( Owner.GroundEntity.IsValid() )
			{
				ApplyFriction( GroundFriction * SurfaceFriction );
			}
		}

		WishVelocity = new Vector3( Owner.InputDirection.x.Clamp(-1f, 1f), Owner.InputDirection.y.Clamp( -1f, 1f ), 0 );
		var inSpeed = WishVelocity.Length.Clamp( 0, 1 );
		WishVelocity *= Owner.ViewAngles.WithPitch(0).ToRotation();

		if ( !Swimming && !IsTouchingLadder )
		{
			WishVelocity = WishVelocity.WithZ( 0 );
		}

		WishVelocity = WishVelocity.Normal * inSpeed;
		WishVelocity *= GetWishSpeed();

		Duck.PreTick();
		
		var stayOnGround = false;

		OnPreTickMove();

		if ( Swimming )
		{
			ApplyFriction( 1 );
			WaterMove();
		}
		else if ( IsTouchingLadder )
		{
			LadderMove();
		}
		else if ( Owner.GroundEntity.IsValid() )
		{
			stayOnGround = true;
			WalkMove();
		}
		else
		{
			AirMove();
		}

		CategorizePosition( stayOnGround );

		if ( !Swimming && !IsTouchingLadder )
		{
			Owner.Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;
		}

		if ( Owner.GroundEntity.IsValid() )
		{
			Owner.Velocity = Owner.Velocity.WithZ( 0 );
		}
	}

	public float GetWishSpeed()
	{
		var ws = Duck.GetWishSpeed();
		if ( ws >= 0 ) return ws;

		if ( EnableSprinting && Input.Down( InputButton.Run ) )
		{
			return SprintSpeed;
		}
		if ( Input.Down( InputButton.Walk ) )
			return WalkSpeed * 0.5f;

		return WalkSpeed;
	}

	private void WalkMove()
	{
		var wishdir = WishVelocity.Normal;
		var wishspeed = WishVelocity.Length;

		WishVelocity = WishVelocity.WithZ( 0 );
		WishVelocity = WishVelocity.Normal * wishspeed;

		Owner.Velocity = Owner.Velocity.WithZ( 0 );
		Accelerate( wishdir, wishspeed, 0, Acceleration );
		Owner.Velocity = Owner.Velocity.WithZ( 0 );
		Owner.Velocity += Owner.BaseVelocity;

		try
		{
			if ( Owner.Velocity.Length < 1.0f )
			{
				Owner.Velocity = Vector3.Zero;
				return;
			}

			var dest = (Owner.Position + Owner.Velocity * Time.Delta).WithZ( Owner.Position.z );
			var pm = TraceBBox( Owner.Position, dest );

			if ( pm.Fraction == 1 )
			{
				Owner.Position = pm.EndPosition;
				StayOnGround();
				return;
			}

			StepMove();
		}
		finally
		{
			Owner.Velocity -= Owner.BaseVelocity;
		}

		StayOnGround();
	}

	private void SetBBox( Vector3 mins, Vector3 maxs )
	{
		if ( Mins == mins && Maxs == maxs )
			return;

		Mins = mins;
		Maxs = maxs;
	}

	private void UpdateBBox()
	{
		var girth = BodyGirth * 0.5f;
		var mins = new Vector3( -girth, -girth, 0 ) * Owner.Scale;
		var maxs = new Vector3( +girth, +girth, BodyHeight ) * Owner.Scale;

		Duck.UpdateBBox( ref mins, ref maxs );

		SetBBox( mins, maxs );
	}

	private void StepMove()
	{
		var startPos = Owner.Position;
		var startVel = Owner.Velocity;

		TryOwnerMove();

		var withoutStepPos = Owner.Position;
		var withoutStepVel = Owner.Velocity;

		Owner.Position = startPos;
		Owner.Velocity = startVel;

		var trace = TraceBBox( Owner.Position, Owner.Position + Vector3.Up * (StepSize + DistEpsilon) );
		if ( !trace.StartedSolid ) Owner.Position = trace.EndPosition;

		TryOwnerMove();

		trace = TraceBBox( Owner.Position, Owner.Position + Vector3.Down * (StepSize + DistEpsilon * 2) );

		if ( !trace.Hit || Vector3.GetAngle( Vector3.Up, trace.Normal ) > GroundAngle )
		{
			Owner.Position = withoutStepPos;
			Owner.Velocity = withoutStepVel;
			return;
		}


		if ( !trace.StartedSolid )
			Owner.Position = trace.EndPosition;

		var withStepPos = Owner.Position;

		float withoutStep = (withoutStepPos - startPos).WithZ( 0 ).Length;
		float withStep = (withStepPos - startPos).WithZ( 0 ).Length;

		if ( withoutStep > withStep )
		{
			Owner.Position = withoutStepPos;
			Owner.Velocity = withoutStepVel;

			return;
		}
	}

	/// <summary>
	/// Add our wish direction and speed onto our velocity.
	/// </summary>
	public virtual void Accelerate( Vector3 wishDir, float wishSpeed, float speedLimit, float acceleration )
	{
		if ( speedLimit > 0 && wishSpeed > speedLimit )
			wishSpeed = speedLimit;

		var currentSpeed = Owner.Velocity.Dot( wishDir );
		var addSpeed = wishSpeed - currentSpeed;

		if ( addSpeed <= 0 )
			return;

		var accelSpeed = acceleration * Time.Delta * wishSpeed * SurfaceFriction;

		if ( accelSpeed > addSpeed )
			accelSpeed = addSpeed;

		Owner.Velocity += wishDir * accelSpeed;
	}

	/// <summary>
	/// Remove ground friction from velocity.
	/// </summary>
	public virtual void ApplyFriction( float frictionAmount = 1.0f )
	{
		var speed = Owner.Velocity.Length;
		if ( speed < 0.1f ) return;

		var control = (speed < StopSpeed) ? StopSpeed : speed;
		var dropAmount = control * Time.Delta * frictionAmount;
		var newSpeed = speed - dropAmount;

		if ( newSpeed < 0 ) newSpeed = 0;

		if ( newSpeed != speed )
		{
			newSpeed /= speed;
			Owner.Velocity *= newSpeed;
		}
	}

	public virtual void AirMove()
	{
		var wishdir = WishVelocity.Normal;
		var wishspeed = WishVelocity.Length;

		Accelerate( wishdir, wishspeed, AirControl, AirAcceleration );

		Owner.Velocity += Owner.BaseVelocity;

		TryOwnerMove();

		Owner.Velocity -= Owner.BaseVelocity;
	}

	public virtual void WaterMove()
	{
		var wishDir = WishVelocity.Normal;
		var wishSpeed = WishVelocity.Length;

		wishSpeed *= 0.8f;

		Accelerate( wishDir, wishSpeed, 100, Acceleration );

		Owner.Velocity += Owner.BaseVelocity;

		TryOwnerMove();

		Owner.Velocity -= Owner.BaseVelocity;
	}

	public virtual void CheckLadder()
	{
		if ( IsTouchingLadder && Input.Pressed( InputButton.Jump ) )
		{
			Owner.Velocity = LadderNormal * 100.0f;
			IsTouchingLadder = false;

			return;
		}

		var ladderDistance = 1.0f;
		var start = Owner.Position;
		var end = start + (IsTouchingLadder ? (LadderNormal * -1.0f) : WishVelocity.Normal) * ladderDistance;

		var pm = Trace.Ray( start, end )
			.Size( Mins, Maxs )
			.WithTag( "ladder" )
			.Ignore( Owner )
			.Run();

		IsTouchingLadder = false;

		if ( pm.Hit )
		{
			IsTouchingLadder = true;
			LadderNormal = pm.Normal;
		}
	}

	public virtual void LadderMove()
	{
		var velocity = WishVelocity;
		var normalDot = velocity.Dot( LadderNormal );
		var cross = LadderNormal * normalDot;

		Owner.Velocity = (velocity - cross) + (-normalDot * LadderNormal.Cross( Vector3.Up.Cross( LadderNormal ).Normal ));

		TryOwnerMove();
	}

	public virtual void TryOwnerMove()
	{
		var mover = new MoveHelper( Owner.Position, Owner.Velocity );
		mover.Trace = mover.Trace.Size( Mins, Maxs ).Ignore( Owner );
		mover.MaxStandableAngle = GroundAngle;
		mover.TryMove( Time.Delta );

		Owner.Position = mover.Position;
		Owner.Velocity = mover.Velocity;
	}

	/// <summary>
	/// Check for a new ground entity.
	/// </summary>
	public virtual void UpdateGroundEntity( TraceResult tr )
	{
		GroundNormal = tr.Normal;

		SurfaceFriction = tr.Surface.Friction * 1.25f;
		if ( SurfaceFriction > 1 ) SurfaceFriction = 1;

		Owner.GroundEntity = tr.Entity;

		if ( Owner.GroundEntity.IsValid() )
		{
			Owner.BaseVelocity = Owner.GroundEntity.Velocity;
		}
	}

	/// <summary>
	/// Try to keep a walking Owner on the ground when running down slopes, etc.
	/// </summary>
	public virtual void StayOnGround()
	{
		var start = Owner.Position + Vector3.Up * 2;
		var end = Owner.Position + Vector3.Down * StepSize;
		var trace = TraceBBox( Owner.Position, start );

		start = trace.EndPosition;
		trace = TraceBBox( start, end );

		if ( trace.Fraction <= 0 ) return;
		if ( trace.Fraction >= 1 ) return;
		if ( trace.StartedSolid ) return;
		if ( Vector3.GetAngle( Vector3.Up, trace.Normal ) > GroundAngle ) return;

		Owner.Position = trace.EndPosition;
	}

	public virtual void OnPreTickMove() { }
	public virtual void AddJumpVelocity() { }

	public virtual void HandleJumping()
	{
		if ( AutoJump ? Input.Down( InputButton.Jump ) : Input.Pressed( InputButton.Jump ) )
		{
			CheckJumpButton();
		}
	}

	public virtual void OnPostCategorizePosition( bool stayOnGround, TraceResult trace ) { }

	protected void CheckJumpButton()
	{
		if ( Swimming )
		{
			ClearGroundEntity();
			Owner.Velocity = Owner.Velocity.WithZ( 100f );

			return;
		}

		if ( !Owner.GroundEntity.IsValid() )
			return;

		ClearGroundEntity();

		var flGroundFactor = 1f;
		var flMul = 268.3281572999747f * 1.2f;
		var startZ = Owner.Velocity.z;

		if ( Duck.IsActive )
			flMul *= 0.8f;

		Owner.Velocity = Owner.Velocity.WithZ( startZ + flMul * flGroundFactor );
		Owner.Velocity -= new Vector3( 0f, 0f, Gravity * 0.5f ) * Time.Delta;

		AddJumpVelocity();
		AddEvent( "jump" );
	}

	private bool CheckStuckAndFix()
	{
		var result = TraceBBox( Owner.Position, Owner.Position );

		if ( !result.StartedSolid )
		{
			StuckTries = 0;
			return false;
		}

		if ( IsClient ) return true;

		var attemptsPerTick = 20;

		for ( int i = 0; i < attemptsPerTick; i++ )
		{
			var pos = Owner.Position + Vector3.Random.Normal * (StuckTries / 2.0f);

			if ( i == 0 )
			{
				pos = Owner.Position + Vector3.Up * 5;
			}

			result = TraceBBox( pos, pos );

			if ( !result.StartedSolid )
			{
				Owner.Position = pos;
				return false;
			}
		}

		StuckTries++;
		return true;
	}

	private void CategorizePosition( bool stayOnGround )
	{
		SurfaceFriction = 1.0f;

		var point = Owner.Position - Vector3.Up * 2;
		var bumpOrigin = Owner.Position;
		var isMovingUpFast = Owner.Velocity.z > MaxNonJumpVelocity;
		var moveToEndPos = false;

		if ( Owner.GroundEntity.IsValid() )
		{
			moveToEndPos = true;
			point.z -= StepSize;
		}
		else if ( stayOnGround )
		{
			moveToEndPos = true;
			point.z -= StepSize;
		}

		if ( isMovingUpFast || Swimming )
		{
			ClearGroundEntity();
			return;
		}

		var pm = TraceBBox( bumpOrigin, point, 4.0f );

		if ( pm.Entity == null || Vector3.GetAngle( Vector3.Up, pm.Normal ) > GroundAngle )
		{
			ClearGroundEntity();
			moveToEndPos = false;

			if ( Owner.Velocity.z > 0 )
				SurfaceFriction = 0.25f;
		}
		else
		{
			UpdateGroundEntity( pm );
		}

		if ( moveToEndPos && !pm.StartedSolid && pm.Fraction > 0.0f && pm.Fraction < 1.0f )
		{
			Owner.Position = pm.EndPosition;
		}

		OnPostCategorizePosition( stayOnGround, pm );
	}

	public bool HasEvent( string eventName )
	{
		if ( Events == null ) return false;
		return Events.Contains( eventName );
	}

	public bool HasTag( string tagName )
	{
		if ( Tags == null ) return false;
		return Tags.Contains( tagName );
	}

	public void AddEvent( string eventName )
	{
		if ( Events == null )
			Events = new HashSet<string>();

		if ( Events.Contains( eventName ) )
			return;

		Events.Add( eventName );
	}

	public void SetTag( string tagName )
	{
		Tags ??= new HashSet<string>();

		if ( Tags.Contains( tagName ) )
			return;

		Tags.Add( tagName );
	}
}
