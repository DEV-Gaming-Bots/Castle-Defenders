using System;
using System.Linq;
using System.Collections.Generic;
using Sandbox;

public partial class BaseNPC : AnimatedEntity
{
	public float BaseHealth;
	public float BaseSpeed { get; set; } = 1;
	public float SpeedMultiplier { get; set; } = 1;
	public int[] MinMaxCashReward => new[] { 1, 2 };
	public int[] MinMaxEXPReward => new[] { 1, 2 };
	public float NPCScale = 1;
	public float Damage = 1;

	[Net] public string NPCNameNet { get; set; }

	public enum PathPriority
	{
		Random,
		Normal,
		Split,
		Alternate,
	}

	[Net] public float ArmourStrength { get; set; } = -1;
	public int SplitAmount => 0;

	public bool IsMinion = false;

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

	public BaseNPCAsset AssetFile;

	public void UseAssetAndSpawn(BaseNPCAsset asset)
	{
		SetModel( "models/citizen/citizen.vmdl" );
		SetupPhysicsFromOBB( PhysicsMotionType.Keyframed, Model.Bounds.Mins, Model.Bounds.Maxs );

		if( asset.OverrideMaterial != null)
			SetMaterialOverride( asset.OverrideMaterial );

		if( !string.IsNullOrEmpty(asset.Hat) )
		{
			ModelEntity hat = new ModelEntity(asset.Hat);
			hat.SetParent( this, true );
		}

		if ( !string.IsNullOrEmpty( asset.Top ) )
		{
			ModelEntity top = new ModelEntity( asset.Top );
			top.SetParent( this, true );
		}

		if ( !string.IsNullOrEmpty( asset.Bottom ) )
		{
			ModelEntity bottom = new ModelEntity( asset.Bottom );
			bottom.SetParent( this, true );
		}

		if ( !string.IsNullOrEmpty( asset.Feet ) )
		{
			ModelEntity feet = new ModelEntity( asset.Feet );
			feet.SetParent( this, true );
		}

		NPCNameNet = asset.Name;
		Health = asset.StartHealth * (GetDifficulty() + CDGame.Instance.LoopedTimes - 1);
		BaseSpeed = asset.Speed;
		Damage = asset.Damage;
		Scale = asset.Scale;

		if ( asset.NPCType == BaseNPCAsset.SpecialType.Armoured )
			ArmourStrength = asset.StartArmor;

		if ( asset.OverrideColor != Color.White)
			RenderColor = asset.OverrideColor;

		CashReward = (int)(Rand.Int( asset.KillReward.MinCash, asset.KillReward.MaxCash ) * ScaleRewards() / 2.25f);
		ExpReward = (int)(Rand.Int( asset.KillReward.MinXP, asset.KillReward.MaxXP ) * ScaleRewards());

		AssetFile = asset;

		Spawn();
	}

	public override void Spawn()
	{
		CurrentEffects = new();

		Tags.Add( "npc" );

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
		Panel = new NPCInfo( NPCNameNet, Health, ArmourStrength);
	}

	[ClientRpc]
	public virtual void UpdateUI()
	{
		Panel.Position = Position + Vector3.Up * 22 * (Scale + 1.35f);
		Panel.Rotation = Rotation;
		Panel.CurHealth = MathF.Round( Health, 2 );
		Panel.CurArmor = MathF.Round( ArmourStrength, 2 );
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

		if ( (Steer != null && AssetFile != null) || !IsValid )
		{
			Steer.Tick( Position );

			_inputVelocity = Steer.Output.Direction.Normal;
			Velocity = Velocity.AddClamped( _inputVelocity, BaseSpeed * SpeedMultiplier );

			Vector3 groundPos = Position;

			var trGr = Trace.Ray( Position, Position + Vector3.Down * 32 * Scale )
				.Ignore( this )
				.EntitiesOnly()
				.Run();

			if ( AssetFile.NPCType == BaseNPCAsset.SpecialType.Airborne )
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
		if ( ArmourStrength > 0 )
		{
			float remainDMG = info.Damage;
			remainDMG -= ArmourStrength;

			ArmourStrength -= info.Damage;
			ArmourStrength = ArmourStrength.Clamp( 0, AssetFile.StartArmor );

			if ( ArmourStrength <= 0 && remainDMG > 0 )
				Health -= remainDMG;
		}
		else
			Health -= info.Damage;

		var attTower = info.Attacker as BaseTower;

		if(attTower.Owner is CDPawn player)
			_lastDmg.Attacker = player;

		if ( Health <= 0 )
			OnKilled();
	}

	public void Split()
	{
		if ( IsMinion )
			return;

		for ( int i = 0; i < 3; i++ )
		{
			BaseNPC minion = new BaseNPC();
			minion.AssetFile = AssetFile;
			minion.NPCNameNet = NPCNameNet + " Minion";
			minion.Position = Position + Vector3.Up * 25 + Vector3.Random.x * 35 + Vector3.Random.y * 35;
			minion.Scale = AssetFile.Scale / 2.0f;

			minion.Model = Model;
			minion.SetupPhysicsFromOBB( PhysicsMotionType.Keyframed, Model.Bounds.Mins, Model.Bounds.Maxs );
			minion.SetMaterialOverride( AssetFile.OverrideMaterial );

			minion.Health = AssetFile.StartHealth / 4;
			minion.BaseSpeed = AssetFile.Speed * 2;
			minion.CastleTarget = CastleTarget;
			minion.Spawn();
			minion.IsMinion = true;

			minion.Steer.Target = Steer.Target;
		}
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

		if ( AssetFile.NPCType == BaseNPCAsset.SpecialType.Splitter )
			Split();

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
