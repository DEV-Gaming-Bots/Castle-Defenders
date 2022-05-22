using Sandbox;
using SandboxEditor;
[Library( "td2_tower_blocker" )]
[SupportsSolid]
[Description( "Prevents spawning of towers" )]
[RenderFields]
public class TowerBlocker : BrushEntity
{
	public override void Spawn()
	{
		EnableDrawing = true;
		UsePhysicsCollision = true;
	}
}
