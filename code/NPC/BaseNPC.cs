using System;
using System.Linq;
using System.Collections.Generic;
using Sandbox;

public partial class BaseNPC : AnimatedEntity
{
	//Basics
	public virtual string NPCName => "Default NPC";
	public virtual string BaseModel => "models/citizen/citizen.vmdl";
	public virtual float BaseHealth => 1;
	public virtual float BaseSpeed { get; set; } = 1;
	public virtual float SpeedMultiplier { get; set; } = 1;
	public virtual int[] MinMaxCashReward => new[] { 1, 2 };
	public virtual int[] MinMaxEXPReward => new[] { 1, 2 };
	public virtual float NPCScale => 1;
	public virtual float Damage => 1;
	public virtual bool IsBoss => false;

	[Net] public string NPCNameNet => NPCName;

	public List<ModelEntity> ClothingEnts;

	public enum SpecialType
	{
		Standard,
		Armoured,
		Hidden,
		Disruptor,
		Splitter,
		Airborne,
	}
	
	public enum PathPriority
	{
		Random,
		Normal,
		Split,
		Alternate,
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
	public PathPriority NextPathPriority;

	public NPCPathSteer Steer;

	private Vector3 _inputVelocity;
	private Vector3 _lookDir;

	public CastleEntity CastleTarget;

	public TimeUntil TimeUntilSpecialRecover;

	public NPCInfo Panel;

	public Vector3 LastNode;
	public Entity CurNode;

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

	public float GetDifficulty()
	{
		switch(CDGame.Instance.Difficulty)
		{
			case CDGame.DiffEnum.Easy: return 1.0f;
			case CDGame.DiffEnum.Medium: return 2.25f;
			case CDGame.DiffEnum.Hard: return 4.5f;
			case CDGame.DiffEnum.Extreme: return 6.75f;
		}

		return 0.0f;
	}

	public float ScaleRewards()
	{
		switch ( CDGame.Instance.Difficulty )
		{
			case CDGame.DiffEnum.Easy: return 0.75f;
			case CDGame.DiffEnum.Medium: return 1.5f;
			case CDGame.DiffEnum.Hard: return 2.25f;
			case CDGame.DiffEnum.Extreme: return 3.0f;
		}

		return 0.0f;
	}

	public override void Spawn()
	{
		SetModel( BaseModel );

		CurrentEffects = new();
		ClothingEnts = new List<ModelEntity>();

		Scale = NPCScale;
		Health = BaseHealth * GetDifficulty() * CDGame.Instance.LoopedTimes;

		CashReward = (int)(Rand.Int( MinMaxCashReward[0], MinMaxCashReward[1] ) * ScaleRewards() / 1.65f);
		ExpReward = (int)(Rand.Int( MinMaxEXPReward[0], MinMaxEXPReward[1] ) * ScaleRewards());

		Tags.Add( "npc" );

		SetupPhysicsFromOBB( PhysicsMotionType.Keyframed, Model.Bounds.Mins, Model.Bounds.Maxs );
		EnableTraceAndQueries = true;
		EnableHitboxes = false;

		Steer = new NPCPathSteer();
		LastNode = Position;
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

	public virtual void ApplyTexture(string matPath, string body = "skin")
	{
		SetMaterialOverride( Material.Load( matPath ), body );
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
		if ( LastNode.IsNearlyZero() )
			Steer.Target = Position;
		else
			Steer.Target = LastNode;
	}

	public virtual void FindNextPath(Vector3 groundPos)
	{
		if ( CDGame.Instance.GameStatus == CDGame.GameEnum.Post )
			Despawn();

		if ( CastleTarget.IsValid() && Position.Distance( CastleTarget.Position ) <= 25.0f )
		{
			var dmgInfo = new DamageInfo { Damage = Damage * GetDifficulty() };

			CastleTarget.DamageCastle( dmgInfo.Damage );
			Despawn();
		}

		foreach ( var path in All.OfType<NPCPath>() )
		{
			if(CurNode == null)
			{
				var nextPath = path.FindNextPath( NextPathPriority );
				CurNode = nextPath;
				Steer.Target = CurNode.Position;
			}

			if ( path.Position.Distance( groundPos ) <= 25.0f )
			{
				LastNode = path.Position;

				var nextPath = path.FindNextPath( NextPathPriority );

				if ( nextPath != null )
				{
					Steer.Target = nextPath.Position;
					CurNode = nextPath;
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

	private void StatusEffectTick()
	{
		if ( CurrentEffects.Count > 0 )
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
							if ( CurNode == null )
								FindNextPath(Position);
							else
								Steer.Target = CurNode.Position;
							break;
					}

					CurrentEffects.Remove( effect );
				}
			}
		}
	}

	//Server ticking for NPC Navigation
	[Event.Tick.Server]
	public virtual void Tick()
	{
		UpdateUI( To.Everyone );

		StatusEffectTick();

		_inputVelocity = 0;

		if ( Steer != null || !IsValid )
		{
			Steer.Tick( Position );

			_inputVelocity = Steer.Output.Direction.Normal;
			Velocity = Velocity.AddClamped( _inputVelocity, BaseSpeed * SpeedMultiplier );

			Vector3 groundPos = Position;

			var trGr = Trace.Ray( Position, Position + Vector3.Down * 32 )
				.Ignore( this )
				.EntitiesOnly()
				.Run();

			if ( NPCType == SpecialType.Airborne )
				groundPos = trGr.EndPosition;

			if ( Steer.Target.Distance( groundPos ) <= 1.0f || Position.Distance( CastleTarget.Position ) <= 25.0f )
				FindNextPath( groundPos );
		}

		if ( TimeUntilSpecialRecover > 0.0f )
			return;

		Move( Time.Delta );

		var walkVelocity = Velocity.WithZ( 0 );
		if ( walkVelocity.Length > 0.5f )
		{
			var turnSpeed = walkVelocity.Length.LerpInverse( 0, 100 );
			var targetRotation = Rotation.LookAt( walkVelocity.Normal, Vector3.Up );
			Rotation = Rotation.Lerp( Rotation, targetRotation, turnSpeed * Time.Delta * 20.0f );
		}

		var animHelper = new NPCAnimationHelper( this );

		_lookDir = Vector3.Lerp( _lookDir, _inputVelocity.WithZ( 0 ) * 1000, Time.Delta * 100.0f );
		animHelper.WithLookAt( EyePosition + _lookDir );
		animHelper.WithVelocity( Velocity );
		animHelper.WithWishVelocity( _inputVelocity );
	}

	protected virtual void Move( float timeDelta )
	{
		var bbox = BBox.FromHeightAndRadius( 16, 4 );

		MoveHelper move = new( Position, Velocity ) { MaxStandableAngle = 50 };
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
			
			if ( _inputVelocity.Length > 0 )
			{
				var movement = move.Velocity.Dot( _inputVelocity.Normal );
				move.Velocity -= movement * _inputVelocity.Normal;
				move.ApplyFriction( tr.Surface.Friction * 10.0f, timeDelta );
				move.Velocity += movement * _inputVelocity.Normal;
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

		Velocity += Input.Rotation * new Vector3( Input.Forward, Input.Left, Input.Up ) * BaseSpeed * SpeedMultiplier * 5 * Time.Delta;
		if ( Velocity.Length > BaseSpeed * SpeedMultiplier ) Velocity = Velocity.Normal * BaseSpeed * SpeedMultiplier;

		Velocity = Velocity.Approach( 0, Time.Delta * BaseSpeed * SpeedMultiplier * 3 );

		Position += Velocity * Time.Delta;

		EyePosition = Position;
	}

	private DamageInfo _lastDmg;

	public override void TakeDamage( DamageInfo info )
	{
		Health -= info.Damage;

		var attTower = info.Attacker as BaseTower;

		if(attTower.Owner is CDPawn player)
			_lastDmg.Attacker = player;

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

		(_lastDmg.Attacker as CDPawn).Client.AddInt( "kills", 1 );

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
