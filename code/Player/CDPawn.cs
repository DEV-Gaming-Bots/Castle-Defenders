using Sandbox;
using System;
using System.ComponentModel;
using System.Linq;

public partial class CDPawn : AnimatedEntity
{
	[ConVar.Client( "cd.music" )]
	public static bool ClientMusic { get; set; } = true;
	[Net, Predicted] public StandardController Controller { get; set; }
	public Vector3 EyePosition
	{
		get => Transform.PointToWorld( EyeLocalPosition );
		set => EyeLocalPosition = Transform.PointToLocal( value );
	}

	[Net, Predicted]
	public Vector3 EyeLocalPosition { get; set; }

	public Rotation EyeRotation
	{
		get => Transform.RotationToWorld( EyeLocalRotation );
		set => EyeLocalRotation = Transform.RotationToLocal( value );
	}

	[Net, Predicted]
	public Rotation EyeLocalRotation { get; set; }

	[ClientInput] public Vector3 InputDirection { get; set; }
	[ClientInput] public Angles ViewAngles { get; set; }
	
	[Net, Predicted]
	public bool ThirdPersonCamera { get; set; }

	public override Ray AimRay => new Ray( EyePosition, EyeRotation.Forward );

	private readonly ClothingContainer _clothing = new();

	private Sound _curMusic;

	private int _scrollInt;
	private int _lastScrollInt;

	[Net]
	public int TotalTowers { get; set; }
	
	[Net]
	public int TowerLimit { get; set; }

	[Net] public bool IsSpecial { get; set; } = true;

	public CDPawn() { }

	public CDPawn( IClient cl ) : this()
	{
		_clothing.LoadFromClient( cl );
	}

	public void SpawnAtLocation()
	{
		var randomSpawnPoint = All.OfType<SpawnPoint>().OrderBy( x => Guid.NewGuid() ).FirstOrDefault();

		if ( randomSpawnPoint != null )
		{
			var tx = randomSpawnPoint.Transform;
			Transform = tx;
		}
	}

	[ClientRpc]
	public void PlayMusic(string music)
	{
		if ( !ClientMusic ) return;

		_curMusic = Sound.FromScreen( music );
	}

	[ClientRpc]
	public void EndMusic( string musicEnd )
	{
		if ( !ClientMusic )
		{
			_curMusic.Stop();
			return;
		}

		_curMusic.Stop();
		_curMusic = Sound.FromScreen( musicEnd );
	}

	[ClientRpc]
	public void PlaySoundOnClient(string sndPath)
	{
		PlaySound( sndPath );
	}

	[ClientRpc]
	public void StopMusic()
	{
		_curMusic.Stop();
	}

	public void SetTowerLimit(int limit)
	{
		if ( limit == 0 )
		{
			TowerLimit = 999;
			return;
		}

		TowerLimit = limit;
		TotalTowers = 0;
	}

	public void CreateHull()
	{
		SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, new Vector3( -16, -16, 0 ), new Vector3( 16, 16, 72 ) );
		EnableHitboxes = true;
	}

	public override void Spawn()
	{
		SetModel( "models/citizen/citizen.vmdl_c" );

		LifeState = LifeState.Alive;
		CurTeam = TeamEnum.Unknown;

		CreateHull();
		Controller = new StandardController(this);
		Tags.Add( "cdplayer" );

		SpawnAtLocation();

		_clothing.DressEntity( this );

		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
	}

	public void GiveSpecialClothing()
	{
		var crown = new ModelEntity( "models/crown.vmdl" );
		crown.SetParent( this, true );
		crown.EnableHideInFirstPerson = true;
	}

	public override void BuildInput()
	{
		var look = Input.AnalogLook;

		var viewAngles = ViewAngles;
		viewAngles += look;
		viewAngles.pitch = viewAngles.pitch.Clamp( -90, 90 );
		ViewAngles = viewAngles.Normal;

		var move = Input.AnalogMove;
		InputDirection = move.Normal;
	}

	public void SimulateCitizenAnimation(IClient cl)
	{
		var controller = Controller;

		if ( controller == null )
			return;

		// where should we be rotated to
		var turnSpeed = 0.02f;

		Rotation rotation;

		// If we're a bot, spin us around 180 degrees.
		if ( cl.IsBot )
			rotation = ViewAngles.WithYaw( ViewAngles.yaw + 180f ).ToRotation();
		else
			rotation = ViewAngles.ToRotation();

		var idealRotation = Rotation.LookAt( rotation.Forward.WithZ( 0 ), Vector3.Up );
		Rotation = Rotation.Slerp( Rotation, idealRotation, Velocity.Length * Time.Delta * turnSpeed );
		Rotation = Rotation.Clamp( idealRotation, 45.0f, out var shuffle ); // lock facing to within 45 degrees of look direction

		CitizenAnimationHelper animHelper = new CitizenAnimationHelper( this );

		animHelper.WithWishVelocity( Velocity );
		animHelper.WithVelocity( Velocity );
		animHelper.WithLookAt( EyePosition + EyeRotation.Forward * 200.0f, 1.0f, 1.0f, 0.5f );
		animHelper.AimAngle = EyeRotation;
		animHelper.FootShuffle = shuffle;
		animHelper.DuckLevel = MathX.Lerp( animHelper.DuckLevel, Input.Down( InputButton.Duck ) ? 1 : 0, Time.Delta * 10.0f );
		animHelper.IsGrounded = GroundEntity != null;
		//animHelper.IsClimbing = controller.HasTag( "climbing" );
		animHelper.IsSwimming = this.GetWaterLevel() >= 0.5f;

		animHelper.HoldType = CitizenAnimationHelper.HoldTypes.None;
		animHelper.AimBodyWeight = 0.5f;
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		Controller?.Simulate();
		SimulateCitizenAnimation( cl );

		//Check if debug is false and we're in an active game
		if ( CDGame.CDDebug == false )
		{
			if ( CDGame.Instance.Competitive && OnOtherTeamSide )
				return;

			if ( CDGame.Instance.GameStatus == CDGame.GameEnum.Active )
				DoTDInputs();
		}
		//Otherwise if debug is on, just do the TDInputs
		else
			DoTDInputs();

		DoTowerOverview();
		TowerSuperRadius();

		if ( CDGame.Instance.DebugMode is CDGame.DebugEnum.Path or CDGame.DebugEnum.All )
		{
			foreach ( var path in All.OfType<NPCPath>() )
			{
				if ( path.FindNormalPath() != null )
					DebugOverlay.Line( path.Position, path.FindNormalPath().Position );

				if ( path.FindSplitPath() != null )
					DebugOverlay.Line( path.Position, path.FindSplitPath().Position );
			}
		}
		
		// Third person
		if ( Input.Pressed( InputButton.View ) )
		{
			ThirdPersonCamera = !ThirdPersonCamera;
		}
	}

	public void TowerSuperRadius()
	{
		if ( CurSuperTower == null )
			return;

		var tr = Trace.Ray( AimRay.Position, AimRay.Position + AimRay.Forward * 145 )
			.WithoutTags( "cdplayer", "npc" )
			.Run();

		ShowSuperRadius( CurSuperTower, tr.EndPosition );
	}
	
	// ---- FOOTSTEPS ----

	TimeSince timeSinceLastFootstep = 0;

	/// <summary>
	/// A footstep has arrived!
	/// </summary>
	public override void OnAnimEventFootstep( Vector3 pos, int foot, float volume )
	{
		if ( LifeState != LifeState.Alive )
			return;

		if ( !Game.IsClient )
			return;

		if ( timeSinceLastFootstep < 0.2f )
			return;

		volume *= FootstepVolume();

		timeSinceLastFootstep = 0;

		//DebugOverlay.Box( 1, pos, -1, 1, Color.Red );
		//DebugOverlay.Text( pos, $"{volume}", Color.White, 5 );

		var tr = Trace.Ray( pos, pos + Vector3.Down * 20 )
			.Radius( 1 )
			.Ignore( this )
			.Run();

		if ( !tr.Hit ) return;

		tr.Surface.DoFootstep( this, tr, foot, volume );
	}

	public virtual float FootstepVolume()
	{
		return Velocity.WithZ( 0 ).Length.LerpInverse( 0.0f, 200.0f ) * 5.0f;
	}

	// ---- FOOTSTEPS END ----

	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );

		Controller?.FrameSimulate();

		Camera.Position = EyePosition;
		Camera.Rotation = ViewAngles.ToRotation();

		// Set field of view to whatever the user chose in options
		Camera.FieldOfView = Screen.CreateVerticalFieldOfView( Game.Preferences.FieldOfView );

		if ( ThirdPersonCamera )
		{
			Camera.FirstPersonViewer = null;
			
			var tr = Trace.Ray( EyePosition, EyePosition + Camera.Rotation.Backward * 90.0f)
				.WithAnyTags( "solid" )
				.Ignore( this )
				.Radius( 8 )
				.Run();

			Camera.Position = tr.EndPosition;
		}
		else
		{
			Camera.FirstPersonViewer = this;
			Camera.Main.SetViewModelCamera( Camera.FieldOfView );
		}
			SimulatePreview();
	}
}
