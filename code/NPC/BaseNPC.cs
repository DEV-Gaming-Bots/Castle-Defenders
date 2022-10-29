using System;
using System.Linq;
using System.Collections.Generic;
using Sandbox;
using System.IO;
using System.Threading.Tasks;

public partial class BaseNPC : AnimatedEntity
{
	//Basics
	public virtual string NPCName => "Default NPC";
	public virtual string BaseModel => "models/citizen/citizen.vmdl";
	public virtual float BaseHealth => 1;
	public virtual float BaseSpeed { get; set; } = 1;
	public virtual float SpeedMultiplier { get; set; } = 1;
	public virtual int[] MinMaxCashReward => new int[] { 1, 2 };
	public virtual int[] MinMaxEXPReward => new int[] { 1, 2 };
	public virtual float NPCScale => 1;
	public virtual float Damage => 1;
	[Net] public string NPCNameNet => NPCName;

	public List<ModelEntity> ClothingEnts;

	public enum SpecialType
	{
		Standard,
		Armoured,
		Hidden,
		Disruptor,
		Splitter,
	}
	
	public enum PathPriority
	{
		Random,
		Normal,
		Split,
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

	public bool ArmourBroken;

	public int CashReward;
	public int ExpReward;
	public PathPriority nextPathPriority;

	public NPCPathSteer Steer;

	Vector3 InputVelocity;
	Vector3 LookDir;

	public CastleEntity CastleTarget;

	public TimeUntil TimeUntilSpecialRecover;

	public NPCInfo Panel;

	Vector3 lastNode;
	Entity curNode;

	public enum PathTeam
	{
		Blue,
		Red
	}

	public PathTeam PathToFollow;

	public enum EffectEnum
	{
		Confusion,
		Frozen,
		Melting,
	}

	public List<(EffectEnum effect, float time, TimeSince timeEffected)> CurrentEffects;

	public int GetDifficulty()
	{
		switch ( CDGame.Instance.Difficulty )
		{
			case CDGame.DiffEnum.Easy:
				return 1;

			case CDGame.DiffEnum.Medium:
				return 3;

			case CDGame.DiffEnum.Hard:
				return 5;

			case CDGame.DiffEnum.Extreme:
				return 8;
		}

		return 0;
	}

	public override void Spawn()
	{
		SetModel( BaseModel );

		CurrentEffects = new();
		ClothingEnts = new List<ModelEntity>();

		Scale = NPCScale;
		Health = BaseHealth * GetDifficulty() * CDGame.Instance.LoopedTimes;

		CashReward = Rand.Int( MinMaxCashReward[0], MinMaxCashReward[1] );
		ExpReward = Rand.Int( MinMaxEXPReward[0], MinMaxEXPReward[1] );

		Tags.Add( "npc" );

		SetupPhysicsFromOBB( PhysicsMotionType.Keyframed, Model.Bounds.Mins, Model.Bounds.Maxs );
		EnableTraceAndQueries = true;
		EnableHitboxes = false;

		Steer = new NPCPathSteer();
		lastNode = Position;
	}

	//When the NPC reaches the castle, despawn
	public void Despawn()
	{
		EnableDrawing = false;
		Delete();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		Panel?.Delete();
		Panel = null;
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		SetUpPanel();
	}

	public virtual void SetUpPanel()
	{
		Panel = new NPCInfo(NPCName, Health);
	}

	[ClientRpc]
	public virtual void UpdateUI()
	{
		Panel.Position = Position + Vector3.Up * 18 * (Scale + 2.0f);
		Panel.Rotation = Rotation;
		Panel.CurHealth = MathF.Round(Health, 2);
	}

	public virtual void OnArmourBroken()
	{
		ArmourBroken = true;
	}

	[ClientRpc]
	public virtual void ApplyTextureClient(string matPath, string body = "skin")
	{
		SetMaterialOverride( Material.Load( matPath ), body );
	}

	public void AddEffect(EffectEnum effect, float time)
	{
		if ( CurrentEffects.Contains( CurrentEffects.FirstOrDefault( x => x.effect == effect ) ) )
			return;

		CurrentEffects.Add( (effect, time, 0.0f) );
	}

	public void GoReversePath()
	{
		if ( lastNode.IsNearlyZero() )
			Steer.Target = Position;
		else
			Steer.Target = lastNode;
	}

	public void FindNextPath()
	{
		if ( CDGame.Instance.GameStatus == CDGame.GameEnum.Post )
			Despawn();

		if ( CastleTarget.IsValid() && Position.Distance( CastleTarget.Position ) <= 25.0f )
		{
			DamageInfo dmgInfo = new DamageInfo();
			dmgInfo.Damage = Damage * GetDifficulty();

			CastleTarget.DamageCastle( dmgInfo.Damage );
			Despawn();
		}

		foreach ( var path in All.OfType<NPCPath>() )
		{
			if(curNode == null)
			{
				var nextPath = path.FindNextPath( nextPathPriority );
				curNode = nextPath;
				Steer.Target = curNode.Position;
			}

			if ( path.Position.Distance( Position ) <= 25.0f )
			{
				lastNode = path.Position;

				var nextPath = path.FindNextPath( nextPathPriority );

				if ( nextPath != null )
				{
					Steer.Target = nextPath.Position;
					curNode = nextPath;
					if ( nextPath is NPCPath nextNpcPath )
					{
						if ( nextNpcPath.TeleportingNode )
						{
							Position = nextNpcPath.Position;
						}

						SpeedMultiplier = nextNpcPath.NodeSpeed;
					}

					break;
				}
			}
		}
	}

	//Server ticking for NPC Navigation
	[Event.Tick.Server]
	public virtual void Tick()
	{
		UpdateUI( To.Everyone );

		if( CurrentEffects.Count > 0 )
		{
			foreach ( var effect in CurrentEffects.ToArray() )
			{
				switch ( effect.effect )
				{
					case EffectEnum.Confusion:
						GoReversePath();
						break;
				}

				if ( effect.timeEffected >= effect.time )
				{
					switch ( effect.effect )
					{
						case EffectEnum.Confusion:
							PlaySound( "confusion_recover" );
							if ( curNode == null )
								FindNextPath();
							else
								Steer.Target = curNode.Position;
							break;
					}

					CurrentEffects.Remove( effect );
				}
			}
		}

		InputVelocity = 0;

		if ( Steer != null || !IsValid )
		{
			Steer.Tick( Position );

			InputVelocity = Steer.Output.Direction.Normal;
			Velocity = Velocity.AddClamped( InputVelocity, BaseSpeed * SpeedMultiplier );

			if ( Steer.Target.Distance( Position ) <= 1.0f || Position.Distance( CastleTarget.Position ) <= 25.0f )
				FindNextPath();
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
		var bbox = BBox.FromHeightAndRadius( 16, 4 );

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
/*		else
		{
			GroundEntity = null;
			move.Velocity += Vector3.Down * 900 * timeDelta;
		}*/

		Position = move.Position;
		Velocity = move.Velocity;
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		Rotation = Input.Rotation;
		EyeRotation = Rotation;

		Velocity += Input.Rotation * new Vector3( Input.Forward, Input.Left, Input.Up ) * BaseSpeed * SpeedMultiplier * 5 * Time.Delta;
		if ( Velocity.Length > BaseSpeed * SpeedMultiplier ) Velocity = Velocity.Normal * BaseSpeed * SpeedMultiplier;

		Velocity = Velocity.Approach( 0, Time.Delta * BaseSpeed * SpeedMultiplier * 3 );

		Position += Velocity * Time.Delta;

		EyePosition = Position;
	}

	DamageInfo lastDMG;

	public override void TakeDamage( DamageInfo info )
	{
		Health -= info.Damage;

		var attTower = info.Attacker as BaseTower;

		if(attTower.Owner is CDPawn player)
			lastDMG.Attacker = player;

		if ( Health <= 0 )
			OnKilled();
	}

	public void Split()
	{

	}

	public override void OnKilled()
	{
		if ( !IsServer )
			return;

		if(CDGame.Instance.Competitive)
		{
			All.OfType<CDPawn>().Where(x => x.CurTeam.ToString().Contains( PathToFollow.ToString() )).ToList().ForEach( x =>
			{
				x.AddCash( CashReward );
				x.AddEXP( ExpReward );
			} );
		} 
		else
		{
			All.OfType<CDPawn>().ToList().ForEach( x =>
			{
				x.AddCash( CashReward );
				x.AddEXP( ExpReward );
			} );
		}

		(lastDMG.Attacker as CDPawn).Client.AddInt( "kills", 1 );

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
