﻿using Sandbox;
using System;
using System.Linq;


public partial class CDPawn : Player
{
	public ClothingContainer Clothing = new();

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
		foreach ( var nearby in FindInSphere( selectedTower.Position, 16 ) )
		{
			if ( nearby is BaseTower tower && tower != selectedTower )
				return false;
		}

		//Second check, look for blocked areas
		foreach ( var nearby in FindInSphere( selectedTower.Position, 8 ) )
		{
			if ( nearby is TowerBlocker )
				return false;
		}
		
		return true;
	}

	public void SimulatePlacement(TraceResult tr)
	{
		if ( Input.Down( InputButton.Reload ) )
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

		if ( !CanPlace( tr ) )
			UpdatePreview( To.Single( this ), tr.EndPosition, new Color(255, 0, 0, 0.5f), Rotation.FromYaw( towerRot ) );
		else
			UpdatePreview( To.Single( this ), tr.EndPosition, new Color( 0, 255, 0, 0.5f ), Rotation.FromYaw( towerRot ) );

		if ( selectedTower != null )
		{
			selectedTower.Position = tr.EndPosition;
			selectedTower.Rotation = Rotation.FromYaw( towerRot );
			selectedTower.IsPreviewing = true;
		}
	}

	public void DoTDInputs()
	{
		if ( IsClient )
			return;

		if ( selectedTower != null )
		{
			var tr = Trace.Ray( EyePosition, EyePosition + EyeRotation.Forward * 105 )
			.Ignore( this )
			.Ignore( selectedTower )
			.Run();

			SimulatePlacement( tr );

			if ( !CanPlace( tr ) )
				return;

			if ( Input.Pressed( InputButton.PrimaryAttack ))
			{
				if ( selectedTower == null )
					return;

				var placedTower = TypeLibrary.Create<BaseTower>( selectedTower.GetType().FullName ); ;

				placedTower.Position = selectedTower.Position;
				placedTower.Rotation = selectedTower.Rotation;
				placedTower.Deploy();

				DestroyPreview(To.Single(this));

				selectedTower.Delete();
				selectedTower = null;
			}
		}

		if ( Input.Pressed( InputButton.SecondaryAttack ) )
		{

			if( selectedTower == null )
			{
				CreatePreview( To.Single( this ), "Sniper" );
				towerRot = 0.0f;
				selectedTower = TypeLibrary.Create<BaseTower>( "Sniper" );
				selectedTower.Owner = this;
				selectedTower.RenderColor = new Color( 255, 255, 255, 0 );
			} 
			else
			{
				DestroyPreview();
				selectedTower.Delete();
				selectedTower = null;
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

		DoTDInputs();
	}
}
