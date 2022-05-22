using System;
using Sandbox;

public partial class TD2Pawn
{
	[ClientRpc]
	public void CreatePreview()
	{
		if ( previewTower != null )
			return;

		previewTower = TypeLibrary.Create<BaseTower>( "Lightning" );
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

		DebugOverlay.Circle( previewTower.Position + Vector3.Up , Rotation.FromPitch( 90f ), previewTower.RangeDistance, Color.Green );
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

