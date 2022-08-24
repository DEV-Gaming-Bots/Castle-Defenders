using Sandbox;
using System;
using System.Linq;


public partial class CDPawn : Player
{
	public ClothingContainer Clothing = new();

	bool topDownView;

	Sound curMusic;

	int scrollInt;

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

	public void SwitchCameraView()
	{
		topDownView = !topDownView;

		if( topDownView )
			CameraMode = new TopDownCamera();
		else
			CameraMode = new FirstPersonCamera();
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
		topDownView = false;

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

		if ( Input.Pressed( InputButton.View ) )
			SwitchCameraView();

		if(CameraMode is TopDownCamera && IsServer)
		{
			Vector2 velo = new Vector2( 0, 0 );

			if (Input.Down(InputButton.Forward))
			{
				velo += new Vector2( 128, EyeRotation.Forward.y );
			}

			if(Input.Down(InputButton.Back))
			{
				velo += new Vector2( -128, EyeRotation.Forward.y );
			}

			if ( Input.Down( InputButton.Right ) )
			{
				velo += new Vector2( EyeRotation.Forward.y, -128 );
			}

			if ( Input.Down( InputButton.Left) )
			{
				velo += new Vector2( EyeRotation.Forward.y, 128 );
			}

			Velocity = velo;
		}

		//Check if debug is false and we're in an active game
		if ( CDGame.Instance.Debug == false )
		{
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

	public void SimulatePreview()
	{
		if ( SelectedTower == null )
			return;

		var tr = Trace.Ray( EyePosition, EyePosition + EyeRotation.Forward * 145 )
			.Ignore( this )
			.WithoutTags( "cdplayer", "tower" )
			.Run();

		if ( !CanPlace( tr ) )
			UpdatePreview( tr.EndPosition, new Color( 255, 0, 0, 0.5f ), SelectedTower.RangeDistance );
		else if (CanPlace( tr ) )
			UpdatePreview( tr.EndPosition, new Color( 0, 255, 0, 0.5f ), SelectedTower.RangeDistance );
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		TowerOverviewClient();
		SimulatePreview();
		TowerSuperRadius();
	}
}
