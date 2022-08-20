using System;
using Sandbox;

public partial class CDPawn
{
	public ModelEntity previewTower;
	public BaseTower SelectedTower;

	float towerRot = 0.0f;

	[ClientRpc]
	public void CreatePreview(string towerModel)
	{
		if ( previewTower != null )
			return;

		previewTower = new ModelEntity();
		previewTower.SetModel( towerModel );
		previewTower.Owner = this;
	}

	[ClientRpc]
	public void UpdatePreview(Vector3 endPos, Color color, Rotation rot, float range)
	{
		if ( previewTower == null )
			return;

		previewTower.Position = endPos;
		previewTower.RenderColor = color;
		previewTower.Rotation = rot;
		previewTower.RenderColor = previewTower.RenderColor.WithAlpha( 0.5f );

		DebugOverlay.Circle( previewTower.Position + Vector3.Up * 5, Rotation.FromPitch(90), range, color.WithAlpha(0.25f));
	}

	[ClientRpc]
	public void DestroyPreview()
	{
		if ( previewTower == null )
			return;

		previewTower.Delete();
		previewTower = null;
	}

	public int GetSelectedSlot()
	{
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

