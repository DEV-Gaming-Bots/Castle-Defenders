using Sandbox;
using System;
using System.ComponentModel;
using System.Linq;


public partial class CDPawn : Player
{
	public ClothingContainer Clothing = new();

	TimeSince timeLastTowerPlace;

	bool isoView = false;

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

	public void SimulatePlacement(TraceResult tr)
	{
		towerRot += Input.MouseWheel * 5f;

		if ( towerRot < 0.0f )
			towerRot = 360.0f;
		else if ( towerRot > 360.0f )
			towerRot = 0.0f;

		if ( !CanPlace( tr ) )
			UpdatePreview( To.Single( this ), tr.EndPosition, new Color(255, 0, 0, 0.5f), Rotation.FromYaw( towerRot ), SelectedTower.RangeDistance );
		else
			UpdatePreview( To.Single( this ), tr.EndPosition, new Color( 0, 255, 0, 0.5f ), Rotation.FromYaw( towerRot ), SelectedTower.RangeDistance );

		if ( SelectedTower != null )
		{
			SelectedTower.Position = tr.EndPosition;
			SelectedTower.Rotation = Rotation.FromYaw( towerRot );
			SelectedTower.IsPreviewing = true;
		}
	}

	public void DoTDInputs()
	{
		if ( GetSelectedSlot() == 0 )
		{
			if ( SelectedTower != null )
			{
				DestroyPreview();
				SelectedTower.Delete();
				SelectedTower = null;
			}
		}

		if ( GetSelectedSlot() >= 1 && timeLastTowerPlace > 0.5f )
		{
			if ( TowerSlots.Length < GetSelectedSlot() )
				return;

			if ( SelectedTower != null )
			{
				DestroyPreview();
				SelectedTower.Delete();
				SelectedTower = null;
			}

			SelectedTower = TypeLibrary.Create<BaseTower>( TowerSlots[GetSelectedSlot() - 1] );
			SelectedTower.Owner = this;
			SelectedTower.RenderColor = new Color( 255, 255, 255, 0 );
			SelectedTower.Spawn();

			if ( Host.IsServer || IsClient )
				CreatePreview( To.Single( this ), SelectedTower.GetModelName() );

			timeLastTowerPlace = 0;
		}

		if ( SelectedTower != null )
		{
			var tr = Trace.Ray( EyePosition, EyePosition + EyeRotation.Forward * 145 )
			.Ignore( this )
			.Ignore( SelectedTower )
			.Size( 1 )
			.Run();

			
			if(Host.IsServer || IsClient)
				SimulatePlacement( tr );

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

	public override void Spawn()
	{
		base.Spawn();
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

		if ( CDGame.Instance.GameStatus == CDGame.GameEnum.Active )
			DoTDInputs();
		
	}
}
