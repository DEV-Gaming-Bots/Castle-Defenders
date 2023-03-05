using Components.popup;
using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Text;

public partial class CDPawn
{
	public ModelEntity PreviewTower { get; set; }

	[Net]
	public BaseTower SelectedTower { get; set; }

	[Net]
	public BaseSuperTower CurSuperTower { get; set; }

	[Net] private float Rot { get; set; }
	private bool _isRotating;

	TimeSince timeLastNotify;

	[ClientRpc]
	public void CreatePreview(string towerModel)
	{
		if ( PreviewTower != null )
			return;

		PreviewTower = new ModelEntity();
		PreviewTower.SetModel( towerModel );
		PreviewTower.Owner = this;
	}

	public void UpdatePreview(Vector3 endPos, Color color, float range, float rot)
	{
		PreviewTower.Position = endPos;
		PreviewTower.Rotation = Rotation.FromYaw( rot );
		PreviewTower.RenderColor = color.WithAlpha(1.0f);

		Circle( endPos + Vector3.Up * 5, Rotation.FromPitch( 90 ), range, Color.Green.WithAlpha( 0.45f ), false );
	}

	public void Circle( Vector3 startPos, Rotation rot, float radius, Color colour, bool hollow = true, int segments = 32, float degrees = 360 )
	{
		var up = rot.Up;
		var right = rot.Right;

		float fsegPi = degrees.DegreeToRadian() / segments;
		
		Vector3 lp = default;

		for ( int i = 0; i <= segments; i++ )
		{
			var x = MathF.Sin( i * fsegPi ) * radius;
			var y = MathF.Cos( i * fsegPi ) * radius;
			var p = startPos + up * x + right * y;
		
			if( hollow )
			{
				if ( i > 0 ) Line( p, lp, colour );
			}
			else
			{
				if ( i > 0 )
				{
					Line( p, startPos, colour );
					Line( p, lp, colour );
				}
			}

			lp = p;
		}
	}

	public void Line( Vector3 startPos, Vector3 endPos, Color colour )
	{
		DebugOverlay.Line( startPos, endPos, colour );
	}

	[ClientRpc]
	public void DestroyPreview()
	{
		if ( PreviewTower == null )
			return;

		PreviewTower.Delete();
		PreviewTower = null;
	}

	public void ResetPlayer()
	{
		TotalTowers = 0;
		DestroyPreview();
	}

	public void ShowSuperRadius(BaseSuperTower superTower, Vector3 pos)
	{
		Circle( pos + Vector3.Up * 5, Rotation.FromPitch( 90 ), superTower.RangeDistance, Color.Green.WithAlpha( 0.75f ), false );
	}

	public void SimulatePreview()
	{
		if ( SelectedTower == null )
			return;

		var tr = Trace.Ray( AimRay.Position, AimRay.Position + AimRay.Forward * 145 )
			.Ignore( this )
			.WithoutTags( "cdplayer", "tower", "npc" )
			.WithAnyTags( "solid", "blocker" )
			.Size( 0.1f )
			.Run();

		Color canPlaceCol = Color.Green.WithAlpha( 0.5f );

		if ( !CanPlace( tr ) )
			canPlaceCol = Color.Red.WithAlpha( 0.5f );

		UpdatePreview( tr.EndPosition, canPlaceCol, SelectedTower.RangeDistance, Rot );
	}

	public void NotifyPlayer(string message)
	{
		if ( timeLastNotify < 1.5f )
			return;

		WindowPopup.CreatePopUp( To.Single(this), message, 3.0f, PopupVertical.Center, PopupHorizontal.Right );
		//ChatBox.AddChatEntry( To.Single( this ), "GAME", message );

		timeLastNotify = 0;
	}

	public bool CanPlace( TraceResult tr )
	{
		if ( tr.Normal.z < 0.99f )
			return false;

		if ( SelectedTower is BaseSuperTower )
		{
			if ( CDGame.Instance.Competitive )
			{
				if ( CurTeam == TeamEnum.Blue && CDGame.Instance.ActiveSuperTowerBlue )
					return false;
				if ( CurTeam == TeamEnum.Red && CDGame.Instance.ActiveSuperTowerRed )
					return false;
			}
			else if ( CDGame.Instance.ActiveSuperTowerBlue )
				return false;
		}

		//First check, look for nearby towers
		foreach ( var nearby in FindInSphere( tr.EndPosition, 24 ) )
		{
			if ( nearby is BaseTower tower && tower != SelectedTower )
				return false;
		}

		//Second check, look for blocked areas
		if ( tr.Entity is TowerBlocker )
			return false;

		return true;
	}

	public void DoTDInputs()
	{
		if ( !Game.IsServer )
			return;

		_isRotating = Input.Down( InputButton.Reload );

		if ( Input.MouseWheel != 0 && !_isRotating )
			_scrollInt -= Input.MouseWheel;

		if ( GetSelectedSlot() > -1 )
			_scrollInt = GetSelectedSlot() - 1;

		if ( _scrollInt > TowerSlots.Count )
			_scrollInt = 0;
		else if (_scrollInt < 0)
			_scrollInt = TowerSlots.Count;

		SetSlotClient( To.Single( this ), _scrollInt );

		if( CurSuperTower != null && _scrollInt == 0 )
			CurSuperTower = null;

		if( CurSuperTower != null )
		{
			var tr = Trace.Ray( AimRay.Position, AimRay.Position + AimRay.Forward * 145 )
			.WithoutTags( "cdplayer", "npc" )
			.Ignore( this )
			.Run();

			if( Input.Pressed(InputButton.PrimaryAttack) )
			{
				CurSuperTower.UseSuperAbility(tr);
				CurSuperTower = null;
				TotalTowers--;
			}

			if( Input.Pressed(InputButton.SecondaryAttack) )
				CurSuperTower = null;
		}

		//Check if the last slot is equal or greater than
		//while checking if the time last placed is greater
		if ( _lastScrollInt != _scrollInt )
		{
			if ( TowerSlots.Count <= _scrollInt )
			{
				_lastScrollInt = _scrollInt;

				if ( SelectedTower != null )
				{
					DestroyPreview();

					SelectedTower?.Delete();
					SelectedTower = null;
				}

				return;
			}

			if ( TowerSlots.Count <= _scrollInt )
			{
				if ( SelectedTower != null )
				{
					DestroyPreview();

					SelectedTower?.Delete();
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

				SelectedTower?.Delete();
				SelectedTower = null;
			}

			SelectedTower = TypeLibrary.Create<BaseTower>( TowerSlots[_scrollInt] );
			SelectedTower.Owner = this;
			SelectedTower.RenderColor = new Color( 255, 255, 255, 0 );

			CreatePreview( To.Single( this ), SelectedTower.GetModelName() );

			_lastScrollInt = _scrollInt;
		} 

		if ( SelectedTower != null )
		{
			var tr = Trace.Ray( AimRay.Position, AimRay.Position + AimRay.Forward * 145 )
			.Ignore( this )
			.WithoutTags( "cdplayer", "tower", "npc" )
			.WithAnyTags( "solid", "blocker" )
			.Size( 0.1f )
			.Run();

			if ( _isRotating )
			{
				Rot += Input.MouseWheel * 15;
				
				if ( Rot > 360 )
					Rot = 0;
				else if ( Rot < 0 )
					Rot = 360;
			}

			SimulatePlacement( tr, Rot );

			if ( Input.Pressed( InputButton.PrimaryAttack ) )
			{
				if ( SelectedTower == null )
					return;

				if(!CanPlace(tr))
				{
					NotifyPlayer( "You can't deploy there!" );
					return;
				}

				if ( GetCash() < SelectedTower.TowerCost )
				{
					NotifyPlayer( "You don't have enough cash to buy this!" );
					return;
				}

				if(TotalTowers >= TowerLimit)
				{
					NotifyPlayer( "You've hit the total tower limit, try selling old towers!" );
					return;
				}


				TakeCash( SelectedTower.TowerCost );

				var placedTower = TypeLibrary.Create<BaseTower>( SelectedTower.GetType().FullName ); ;

				placedTower.Position = SelectedTower.Position;
				placedTower.Rotation = SelectedTower.Rotation;

				placedTower.IsPreviewing = false;
				placedTower.Owner = this;
				placedTower.Spawn();
				placedTower.PlayDeployAnimRPC();
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
		if ( SelectedTower.IsValid() )
			return;

		var tr = Trace.Ray( AimRay.Position, AimRay.Position + AimRay.Forward * 145 )
			.UseHitboxes()
			.WithTag( "tower" )
			.Run();

		if ( tr.Entity is BaseTower tower && tower.Owner == this && tr.Entity is not BaseSuperTower )
		{
			if ( Input.Pressed( InputButton.PrimaryAttack ) )
				tower.UpgradeTower();

			if ( Input.Pressed( InputButton.SecondaryAttack ) )
			{ 
				tower.SellTower();
				TotalTowers--;
			}

			if ( Input.Pressed( InputButton.Use ) )
				tower.SetNextPriority();
		}

		if ( tr.Entity is BaseSuperTower superTower && superTower.Owner == this )
		{
			if ( superTower.TimeSinceDeployed < superTower.DeploymentTime )
				return;

			if( Input.Pressed( InputButton.PrimaryAttack ) )
				CurSuperTower = superTower;

			if ( Input.Pressed( InputButton.SecondaryAttack ) )
				superTower.SellTower();
		}
	}

	public void SimulatePlacement( TraceResult tr, float rot )
	{
		if ( SelectedTower.IsValid() )
		{
			SelectedTower.Position = tr.EndPosition;
			SelectedTower.Rotation = Rotation.FromYaw(rot);
		}
	}

	public int GetSelectedSlot()
	{
		if ( Game.IsClient )
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

