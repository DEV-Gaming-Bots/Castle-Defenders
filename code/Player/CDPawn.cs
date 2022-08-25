using Sandbox;
using System;
using System.Linq;


public partial class CDPawn : Player
{
	public ClothingContainer Clothing = new();

	Sound curMusic;

	int scrollInt;
	int lastScrollInt;

	public CDPawn()
	{

	}

	public CDPawn( Client cl ) : this()
	{
		Clothing.LoadFromClient( cl );
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
		curMusic = Sound.FromScreen( music );
	}

	[ClientRpc]
	public void EndMusic( string musicEnd )
	{
		curMusic.Stop();
		curMusic = Sound.FromScreen( musicEnd );
	}

	[ClientRpc]
	public void PlaySoundOnClient(string sndPath)
	{
		PlaySound( sndPath );
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
		Clothing.DressEntity( this );

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

		if ( CDGame.Instance.DebugMode == CDGame.DebugEnum.Path || CDGame.Instance.DebugMode == CDGame.DebugEnum.All )
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
			.UseHitboxes( true )
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
