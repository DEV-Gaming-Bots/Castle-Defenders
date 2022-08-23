using System;
using System.Text;
using Sandbox;

public partial class CDPawn
{
	public ModelEntity PreviewTower;

	public BaseTower SelectedTower;

	public BaseSuperTower CurSuperTower;

	float towerRot = 0.0f;


	[ClientRpc]
	public void CreatePreview(string towerModel)
	{
		if ( PreviewTower != null )
			return;

		PreviewTower = new ModelEntity();
		PreviewTower.SetModel( towerModel );
		PreviewTower.Owner = this;
	}

	[ClientRpc]
	public void UpdatePreview(Vector3 endPos, Color color, Rotation rot, float range)
	{
		if ( PreviewTower == null )
			return;

		PreviewTower.Position = endPos;
		PreviewTower.RenderColor = color;
		PreviewTower.Rotation = rot;
		PreviewTower.RenderColor = PreviewTower.RenderColor.WithAlpha( 0.5f );

		DebugOverlay.Circle( PreviewTower.Position + Vector3.Up * 5, Rotation.FromPitch(90), range, color.WithAlpha(0.25f));
	}

	[ClientRpc]
	public void DestroyPreview()
	{
		if ( PreviewTower == null )
			return;

		PreviewTower.Delete();
		PreviewTower = null;
	}

	[ClientRpc]
	public void ShowRadius(BaseTower tower)
	{
		DebugOverlay.Circle( tower.Position + Vector3.Up * 5, Rotation.FromPitch( 90 ), tower.RangeDistance, Color.Blue.WithAlpha( 0.30f ) );
	}

	[ClientRpc]
	public void ShowSuperRadius(BaseSuperTower superTower, Vector3 pos)
	{
		DebugOverlay.Circle( pos + Vector3.Up * 5, Rotation.FromPitch( 90 ), superTower.RangeDistance, Color.Blue.WithAlpha( 0.30f ) );
	}

	public bool CanPlace( TraceResult tr )
	{
		if ( tr.Normal.z < 0.99 )
			return false;

		if( !SelectedTower.IsValid() )
		{
			SelectedTower = null;
			return false;
		}

		if ( SelectedTower is BaseSuperTower && CDGame.Instance.ActiveSuperTower == true )
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

			if( CurSuperTower != null )
				CurSuperTower = null;
		}

		if( CurSuperTower != null )
		{
			var tr = Trace.Ray( EyePosition, EyePosition + EyeRotation.Forward * 145 )
			.WithoutTags( "cdplayer" )
			.Run();

			ShowSuperRadius( To.Single(this), CurSuperTower, tr.EndPosition );

			if( Input.Pressed(InputButton.PrimaryAttack) )
			{
				CurSuperTower.UseSuperAbility(tr);
				CurSuperTower = null;
			}
		}

		//Check if the last slot is equal or greater than
		//while checking if the time last placed is greater
		if ( GetSelectedSlot() > 0 && timeLastTowerPlace > 0.5f )
		{
			//If the player is past their slots, stop here
			if ( TowerSlots.Length <= GetSelectedSlot() - 1 )
				return;

			//If the player has a super tower selected, nullify that tower
			if ( CurSuperTower != null )
				CurSuperTower = null;

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
			.Ignore( SelectedTower )
			.WithoutTags( "cdplayer" )
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
	public void DoTowerOverview()
	{
		//We have a selected tower in preview, stop here
		if ( SelectedTower != null )
			return;

		var tr = Trace.Ray( EyePosition, EyePosition + EyeRotation.Forward * 145 )
			.UseHitboxes( true )
			.WithTag( "tower" )
			.Run();

		if ( tr.Entity is BaseTower tower && tower.Owner == this && tr.Entity is not BaseSuperTower )
		{
			if ( IsServer )
				ShowRadius( To.Single( this ), tower );

			if ( tower.TimeSinceDeployed < tower.DeploymentTime )
				return;

			if ( tower.TimeLastUpgrade < 4.0f )
				return;

			if ( Input.Pressed( InputButton.PrimaryAttack ) )
			{
				if ( tower.CanUpgrade() )
					tower.UpgradeTower();
			}

			if ( Input.Pressed( InputButton.SecondaryAttack ) )
				tower.SellTower();
		}

		if ( tr.Entity is BaseSuperTower superTower && superTower.Owner == this )
		{
			if ( superTower.TimeSinceDeployed < superTower.DeploymentTime )
				return;

			if( Input.Pressed( InputButton.PrimaryAttack ) )
				CurSuperTower = superTower;
		}
	}

	public void SimulatePlacement( TraceResult tr )
	{
		towerRot += Input.MouseWheel * 5f;

		if ( towerRot < 0.0f )
			towerRot = 360.0f;
		else if ( towerRot > 360.0f )
			towerRot = 0.0f;

		if ( !CanPlace( tr ) )
			UpdatePreview( To.Single( this ), tr.EndPosition, new Color( 255, 0, 0, 0.5f ), Rotation.FromYaw( towerRot ), SelectedTower.RangeDistance );
		else
			UpdatePreview( To.Single( this ), tr.EndPosition, new Color( 0, 255, 0, 0.5f ), Rotation.FromYaw( towerRot ), SelectedTower.RangeDistance );

		if ( SelectedTower.IsValid() )
		{
			SelectedTower.Position = tr.EndPosition;
			SelectedTower.Rotation = Rotation.FromYaw( towerRot );
			SelectedTower.IsPreviewing = true;
		}
	}

	public int GetSelectedSlot()
	{
		if ( IsClient )
			return -1;

		if ( Input.Pressed( InputButton.Slot1 ) )
			return 1;
		if ( Input.Pressed( InputButton.Slot2 ) )
			return 2;
		if ( Input.Pressed( InputButton.Slot3 ) )
			return 3;
		if ( Input.Pressed( InputButton.Slot4 ) )
			return 4;
		if ( Input.Pressed( InputButton.Slot5 ) )
			return 5;
		if ( Input.Pressed( InputButton.Slot6 ) )
			return 6;
		if ( Input.Pressed( InputButton.Slot7 ) )
			return 7;
		if ( Input.Pressed( InputButton.Slot0 ) )
			return 0;

		return -1;
	}
}

