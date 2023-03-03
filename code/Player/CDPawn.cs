using Sandbox;
using System;
using System.ComponentModel;
using System.Linq;

public partial class CDPawn : AnimatedEntity
{
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

	public override Ray AimRay => new Ray( EyePosition, EyeRotation.Forward );

	private readonly ClothingContainer _clothing = new();

	private Sound _curMusic;

	private int _scrollInt;
	private int _lastScrollInt;

	[Net]
	public int TotalTowers { get; set; }
	
	[Net]
	public int TowerLimit { get; set; }

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
		_curMusic = Sound.FromScreen( music );
	}

	[ClientRpc]
	public void EndMusic( string musicEnd )
	{
		_curMusic.Stop();
		_curMusic = Sound.FromScreen( musicEnd );
	}

	[ClientRpc]
	public void PlaySoundOnClient(string sndPath)
	{
		PlaySound( sndPath );
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

	public void SimulateCitizenAnimation()
	{
		var animHelper = new CitizenAnimationHelper( this );
		animHelper.AimAngle = ViewAngles.ToRotation();
		animHelper.IsGrounded = GroundEntity != null;

		animHelper.WithLookAt(AimRay.Position + AimRay.Forward);
		animHelper.WithVelocity( Velocity );
		animHelper.WithWishVelocity( Controller.WishVelocity );

		Rotation rot = ViewAngles.ToRotation();
		rot.x = 0;
		rot.y = 0;

		LocalRotation = rot;
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		Controller?.Simulate();
		SimulateCitizenAnimation();

		//Check if debug is false and we're in an active game
		if ( CDGame.Debug == false )
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

		//SimulatePreview();
	}

	public void TowerSuperRadius()
	{
		if ( CurSuperTower == null )
			return;

		var tr = Trace.Ray( AimRay.Position, AimRay.Forward * 145 )
			.WithoutTags( "cdplayer", "npc" )
			.Run();

		ShowSuperRadius( CurSuperTower, tr.EndPosition );
	}

	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );

		Controller?.FrameSimulate();

		Camera.Position = EyePosition;
		Camera.Rotation = ViewAngles.ToRotation();

		// Set field of view to whatever the user chose in options
		Camera.FieldOfView = Screen.CreateVerticalFieldOfView( Game.Preferences.FieldOfView );

		// Set the first person viewer to this, so it won't render our model
		Camera.FirstPersonViewer = this;
		Camera.Main.SetViewModelCamera( Camera.FieldOfView );

		SimulatePreview();
	}
}
