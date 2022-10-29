﻿using System;
using System.Security.Cryptography;
using System.Text;
using Sandbox;

public partial class CDPawn
{
	public ModelEntity PreviewTower;
	public ModelEntity TowerInHand;

	[Net]
	public BaseTower SelectedTower { get; set; }

	[Net]
	public BaseSuperTower CurSuperTower { get; set; }

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
	public void UpdatePreview(Vector3 endPos, Color color, float range)
	{
		if ( PreviewTower == null )
			return;

		PreviewTower.Position = endPos;
		PreviewTower.RenderColor = color.WithAlpha(1.0f);

		//Rect rect = new Rect( PreviewTower.Position, new Vector2(64, 64) );

		//Log.Info( rect.Width );

		//Graphics.DrawQuad( rect, Material.Load( "materials/dev/debug_wireframe.vmat" ), color );
		//Graphics.DrawRoundedRectangle( rect, color.WithAlpha( 0.25f ) );

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

	public void ShowRadius(BaseTower tower)
	{
		DebugOverlay.Circle( tower.Position + Vector3.Up * 5, Rotation.FromPitch( 90 ), tower.RangeDistance, Color.Blue.WithAlpha(0.2f) );
	}

	public void ShowSuperRadius(BaseSuperTower superTower, Vector3 pos)
	{
		DebugOverlay.Circle( pos + Vector3.Up * 5, Rotation.FromPitch( 90 ), superTower.RangeDistance, Color.Blue.WithAlpha( 0.30f ) );
	}
	public void SimulatePreview()
	{
		if ( SelectedTower == null )
			return;

		var tr = Trace.Ray( EyePosition, EyePosition + EyeRotation.Forward * 145 )
			.Ignore( this )
			.WithoutTags( "cdplayer", "tower", "npc" )
			.Run();

		if ( !CanPlace( tr ) )
			UpdatePreview( tr.EndPosition, new Color( 255, 0, 0, 0.5f ), SelectedTower.RangeDistance );
		else if ( CanPlace( tr ) )
			UpdatePreview( tr.EndPosition, new Color( 0, 255, 0, 0.5f ), SelectedTower.RangeDistance );
	}

	public bool CanPlace( TraceResult tr )
	{
		if ( tr.Normal.z < 0.99 )
			return false;

		if ( TotalTowers >= TowerLimit )
			return false;

		if ( GetCash() < SelectedTower.TowerCost )
			return false;

		if ( SelectedTower is BaseSuperTower )
		{
			if ( CDGame.Instance.Competitive )
			{
				if ( CurTeam == TeamEnum.Blue && CDGame.Instance.ActiveSuperTowerBlue )
					return false;
				else if ( CurTeam == TeamEnum.Red && CDGame.Instance.ActiveSuperTowerRed )
					return false;
			}
			else if ( CDGame.Instance.ActiveSuperTowerBlue )
				return false;
		}

		//First check, look for nearby towers
		foreach ( var nearby in FindInSphere( tr.EndPosition, 24 ) )
		{
			if ( nearby is BaseTower tower && tower != SelectedTower )
			{
				return false;
			}
		}

		//Second check, look for blocked areas
		if ( tr.Entity is TowerBlocker )
			return false;

		return true;
	}

	public void DoTDInputs()
	{
		if ( !IsServer )
			return;

		if ( Input.MouseWheel != 0)
			scrollInt -= Input.MouseWheel;

		if ( GetSelectedSlot() > -1 )
			scrollInt = GetSelectedSlot() - 1;

		if ( scrollInt > TowerSlots.Length )
			scrollInt = 0;
		else if (scrollInt < 0)
			scrollInt = TowerSlots.Length;

		//0 = empty handed
		
		SetSlotClient( To.Single( this ), scrollInt );

		if( CurSuperTower != null && scrollInt == 0 )
			CurSuperTower = null;

		if( CurSuperTower != null )
		{
			var tr = Trace.Ray( EyePosition, EyePosition + EyeRotation.Forward * 145 )
			.WithoutTags( "cdplayer", "npc" )
			.Run();

			if( Input.Pressed(InputButton.PrimaryAttack) )
			{
				CurSuperTower.UseSuperAbility(tr);
				CurSuperTower = null;
				TotalTowers--;
			}

			if(Input.Pressed(InputButton.SecondaryAttack))
				CurSuperTower = null;
		}
		//Check if the last slot is equal or greater than
		//while checking if the time last placed is greater
		if ( lastScrollInt != scrollInt)
		{
			if ( TowerSlots.Length <= scrollInt )
			{
				lastScrollInt = scrollInt;

				if ( SelectedTower != null )
				{
					DestroyPreview();

					SelectedTower.Delete();
					SelectedTower = null;

					TowerInHand.Delete();
					TowerInHand = null;
				}
				return;
			}

			if ( TowerSlots.Length <= scrollInt )
			{
				if ( SelectedTower != null )
				{
					DestroyPreview();

					SelectedTower.Delete();
					SelectedTower = null;
				}
			}

			//If the player has a super tower selected, nullify that tower
			if ( CurSuperTower != null )
				CurSuperTower = null;

			//If the player has a selected tower, destroy preview, delete and nullify
			if ( SelectedTower != null )
			{
				DestroyPreview();

				SelectedTower.Delete();
				SelectedTower = null;
			}

			SelectedTower = TypeLibrary.Create<BaseTower>( TowerSlots[scrollInt] );
			SelectedTower.Owner = this;
			SelectedTower.RenderColor = new Color( 255, 255, 255, 0 );
			SelectedTower.Spawn();

			if( TowerInHand != null )
			{
				TowerInHand.Delete();
				TowerInHand = null;
			}

			TowerInHand = new ModelEntity( SelectedTower.GetModelName() );

			TowerInHand.SetParent(this, true);
			TowerInHand.Spawn();
			TowerInHand.LocalScale = 0.5f;
			TowerInHand.EnableHideInFirstPerson = true;

			CreatePreview( To.Single( this ), SelectedTower.GetModelName() );

			lastScrollInt = scrollInt;
		} 

		if ( SelectedTower != null )
		{
			var tr = Trace.Ray( EyePosition, EyePosition + EyeRotation.Forward * 145 )
			.WithoutTags( "cdplayer", "tower", "npc" )
			.Size( 0.1f )
			.Run();

			if ( SelectedTower != null )
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
				placedTower.Owner = this;

				placedTower.Spawn();
				TotalTowers++;
			}
		}
	}

	[ClientRpc]
	public void SetSlotClient(int i)
	{
		PlayerLoadout.SetSlot( i );
	}

	public void DoTowerOverview()
	{

		//We have a selected tower in preview, stop here
		if ( SelectedTower.IsValid() || TowerInHand.IsValid())
			return;

		var tr = Trace.Ray( EyePosition, EyePosition + EyeRotation.Forward * 145 )
			.UseHitboxes( true )
			.WithTag( "tower" )
			.Run();

		if ( tr.Entity is BaseTower tower && tower.Owner == this && tr.Entity is not BaseSuperTower )
		{
			if ( Input.Pressed( InputButton.PrimaryAttack ) && tower.CanUpgrade() )
				tower.UpgradeTower();

			if ( Input.Pressed( InputButton.SecondaryAttack ) )
			{ 
				tower.SellTower();
				TotalTowers--;
			}
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
		if ( SelectedTower.IsValid() )
		{
			SelectedTower.Position = tr.EndPosition;
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

