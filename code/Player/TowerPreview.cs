using System;
using Sandbox;

public partial class CDPawn
{
	public ModelEntity PreviewTower;

	public BaseTower SelectedTower;

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

