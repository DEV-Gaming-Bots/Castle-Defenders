using System;
using Sandbox;

public partial class CDPawn
{
	public BaseTower previewTower;
	public BaseTower selectedTower;

	float towerRot = 0.0f;

	[ClientRpc]
	public void CreatePreview(string towerName)
	{
		if ( previewTower != null )
			return;

		previewTower = TypeLibrary.Create<BaseTower>( towerName );
		previewTower.Owner = this;
	}

	[ClientRpc]
	public void UpdatePreview(Vector3 endPos, Color color, Rotation rot)
	{
		if ( previewTower == null )
			return;

		previewTower.Position = endPos;
		previewTower.RenderColor = color;
		previewTower.Rotation = rot;
		previewTower.RenderColor = previewTower.RenderColor.WithAlpha( 0.5f );

		color.a /= 2;

		DebugOverlay.Circle( previewTower.Position + Vector3.Up , Rotation.FromPitch( 90f ), previewTower.RangeDistance, color );
	}

	[ClientRpc]
	public void DestroyPreview()
	{
		if ( previewTower == null )
			return;

		previewTower.Delete();
		previewTower = null;
	}

	[Event.Tick.Client]
	public void SimulateClient()
	{
		if ( previewTower == null || !previewTower.Owner.IsValid() )
			return;
	}
}

