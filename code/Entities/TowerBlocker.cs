using Sandbox;
using SandboxEditor;

[Library( "cd_tower_blocker" )]
[Title( "Tower Blocker" ), Description( "Prevents spawning of towers" )]
[Solid]
[RenderFields]
[HammerEntity]
public class TowerBlocker : ModelEntity
{
	public override void Spawn()
	{
		EnableDrawing = false;
	}
}
