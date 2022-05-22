using Sandbox;
using System;
using System.Linq;


public partial class TD2Pawn : Player
{
	public ClothingContainer Clothing = new();

	bool isoView = false;

	public BaseTower previewTower;
	public BaseTower selectedTower;

	float towerRot = 0.0f;

	public TD2Pawn()
	{

	}

	public TD2Pawn( Client cl ) : this()
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

	public void DoTDInputs()
	{
		if ( IsClient )
			return;

		//TEMPORARY, WILL REMOVE LATER
		
		var tr = Trace.Ray( EyePosition, EyePosition + EyeRotation.Forward * 105 )
			.Ignore(this)
			.Run();

		if(Input.Down(InputButton.Reload))
		{
			if ( Input.Down( InputButton.Walk ) )
				towerRot -= 5f;
			else
				towerRot += 5f;
		}

		if ( towerRot < 0.0f )
			towerRot = 360.0f;
		else if ( towerRot > 360.0f )
			towerRot = 0.0f;

		if( tr.Normal.z == 1 || tr.Entity is TowerBlocker )
			UpdatePreview(To.Single(this), tr.EndPosition, Color.Green, Rotation.FromYaw( towerRot ));
		else
			UpdatePreview( To.Single( this ), tr.EndPosition, Color.Red, Rotation.FromYaw( towerRot ) );

		if ( selectedTower != null )
		{
			selectedTower.Position = tr.EndPosition;
			selectedTower.Rotation = Rotation.FromYaw( towerRot );
			selectedTower.IsPreviewing = true;
		}

		if ( Input.Pressed( InputButton.PrimaryAttack ))
		{
			if ( selectedTower == null || tr.Normal.z != 1 || tr.Entity is TowerBlocker )
				return;

			foreach ( var nearTower in FindInSphere(selectedTower.Position + Vector3.Up * 8, 32) )
			{
				if ( nearTower.IsValid() && nearTower != selectedTower )
					return;
			}

			var placedTower = TypeLibrary.Create<BaseTower>( "Lightning" ); ;

			placedTower.Position = selectedTower.Position;
			placedTower.Rotation = selectedTower.Rotation;
			placedTower.Deploy();
			placedTower.RenderColor = new Color( 255, 255, 255, 1 );

			DestroyPreview(To.Single(this));

			selectedTower.Delete();
			selectedTower = null;
		}

		if ( Input.Pressed( InputButton.SecondaryAttack ) && selectedTower == null )
		{
			CreatePreview( To.Single( this ) );
			towerRot = 0.0f;
			selectedTower = TypeLibrary.Create<BaseTower>( "Lightning" );
			selectedTower.Owner = this;
			selectedTower.RenderColor = new Color( 255, 255, 255, 0 );
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

		DoTDInputs();
	}
}
