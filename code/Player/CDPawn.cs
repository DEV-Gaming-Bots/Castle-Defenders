using Sandbox;
using System;
using System.ComponentModel;
using System.Linq;


public partial class CDPawn : Player
{
	public ClothingContainer Clothing = new();

	TimeSince timeLastTowerPlace;

	bool isoView = false;

	Sound curMusic;

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
		isoView = !isoView;

		if( isoView )
		{
			//TODO: Isometric view
		} 
		else if (!isoView )
		{
			CameraMode = new FirstPersonCamera();
		}
	}

	public bool CanPlace(TraceResult tr)
	{
		if ( tr.Normal.z != 1 )
			return false;

		//First check, look for nearby towers
		foreach ( var nearby in FindInSphere( SelectedTower.Position, 16 ) )
		{
			if ( nearby is BaseTower tower && tower != SelectedTower )
				return false;
		}

		//Second check, look for blocked areas
		if ( tr.Entity is TowerBlocker || tr.Entity is BaseNPC )
			return false;
		
		return true;
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

	public override void Spawn()
	{
		EnableLagCompensation = true;

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
	}

	public void DoTowerOverview()
	{
		//We have a selected tower in preview, stop here
		if ( SelectedTower != null )
			return;

		var tr = Trace.Ray( EyePosition, EyePosition + EyeRotation.Forward * 145 )
			.UseHitboxes( true )
			.WithTag( "tower" )
			.Run();

		if(tr.Entity is BaseTower tower && tower.Owner == this)
		{
			if ( tower.TimeSinceDeployed < tower.DeploymentTime )
				return;

			if ( tower.TimeLastUpgrade < 4.0f )
				return;

			if(Input.Pressed(InputButton.PrimaryAttack))
			{
				if ( tower.CanUpgrade() )
					tower.UpgradeTower();
			}

			if ( Input.Pressed( InputButton.SecondaryAttack ) )
				tower.SellTower();
		}
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );
	}

	public void DoTDInputs()
	{
		if ( !IsServer )
			return;

		//0 = empty handed
		if ( GetSelectedSlot() == 0 )
		{
			if ( SelectedTower != null )
			{
				DestroyPreview();
				SelectedTower.Delete();
				SelectedTower = null;
			}
		}

		//Check if the last slot is equal or greater than
		//while checking if the time last placed is greater
		if ( GetSelectedSlot() > 0 && timeLastTowerPlace > 0.5f )
		{
			//If the player is past their slots, stop here
			if ( TowerSlots.Length <= GetSelectedSlot() - 1 )
				return;

			//If the player has a selected tower, destroy preview, delete and nullify
			if ( SelectedTower != null && IsServer )
			{
				DestroyPreview();
				SelectedTower.Delete();
				SelectedTower = null;
			}

			SelectedTower = TypeLibrary.Create<BaseTower>( TowerSlots[GetSelectedSlot() - 1] );
			SelectedTower.Owner = this;
			SelectedTower.RenderColor = new Color( 255, 255, 255, 0 );
			SelectedTower.Spawn();

			CreatePreview( To.Single( this ), SelectedTower.GetModelName() );

			timeLastTowerPlace = 0;
		}

		if ( SelectedTower != null )
		{
			var tr = Trace.Ray( EyePosition, EyePosition + EyeRotation.Forward * 145 )
			.Ignore( this )
			.Ignore( SelectedTower )
			.Size( 0.1f )
			.Run();

			if ( SelectedTower != null )
				SimulatePlacement( tr );

			else if ( IsServer )
			{
				SelectedTower?.Delete();
				SelectedTower = null;
			}

			if ( !CanPlace( tr ) )
				return;

			if ( Input.Pressed( InputButton.PrimaryAttack ) )
			{
				if ( SelectedTower == null )
					return;

				if ( GetCash() < SelectedTower.TowerCost )
					return;

				TakeCash( SelectedTower.TowerCost );

				var placedTower = TypeLibrary.Create<BaseTower>( SelectedTower.GetType().FullName ); ;

				placedTower.Position = SelectedTower.Position;
				placedTower.Rotation = SelectedTower.Rotation;
				placedTower.IsPreviewing = false;
				placedTower.Owner = this;

				placedTower.Spawn();

				DestroyPreview( To.Single( this ) );

				if ( IsServer )
				{
					SelectedTower.Delete();
					SelectedTower = null;
				}
			}
		}
	}
}
