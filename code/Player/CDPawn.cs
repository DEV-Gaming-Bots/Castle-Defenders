using Sandbox;
using System;
using System.Linq;

public sealed partial class CDPawn : Player
{
	private readonly ClothingContainer _clothing = new();

	private Sound _curMusic;

	private int _scrollInt;
	private int _lastScrollInt;

	[Net]
	public int TotalTowers { get; set; }
	
	[Net]
	public int TowerLimit { get; set; }

	public CDPawn() { }

	public CDPawn( Client cl ) : this()
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

	public override void Spawn()
	{
		EnableLagCompensation = true;

		LifeState = LifeState.Alive;
		CurTeam = TeamEnum.Unknown;

		CreateHull();
		Tags.Add( "cdplayer" );

		SpawnAtLocation();

		SetModel( "models/citizen/citizen.vmdl_c" );
		_clothing.DressEntity( this );

		CameraMode = new FirstPersonCamera();
		Animator = new StandardPlayerAnimator();
		Controller = new WalkController();

		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		//Check if debug is false and we're in an active game
		if ( CDGame.Instance.Debug == false )
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
	}

	public void TowerOverviewClient()
	{
		if ( SelectedTower != null )
			return;

		var tr = Trace.Ray( EyePosition, EyePosition + EyeRotation.Forward * 145 )
			.UseHitboxes()
			.WithTag( "tower" )
			.Run();

		if ( tr.Entity is BaseTower tower && tower.Owner == this && tr.Entity is not BaseSuperTower )
			ShowRadius( tower );
	}

	public void TowerSuperRadius()
	{
		if ( CurSuperTower == null )
			return;

		var tr = Trace.Ray( EyePosition, EyePosition + EyeRotation.Forward * 145 )
			.WithoutTags( "cdplayer", "npc" )
			.Run();

		ShowSuperRadius( CurSuperTower, tr.EndPosition );
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		TowerOverviewClient();
		SimulatePreview();
		TowerSuperRadius();
	}
}
