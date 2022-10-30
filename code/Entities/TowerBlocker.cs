using Sandbox;
using SandboxEditor;

[Library( "cd_tower_blocker" )]
[Title( "Tower Blocker" ), Description( "Prevents spawning of towers" )]
[Solid, RenderFields]
[HammerEntity]
public sealed class TowerBlocker : ModelEntity
{
	public override void Spawn()
	{
		base.Spawn();
		EnableDrawing = false;
		SetupPhysicsFromModel( PhysicsMotionType.Static );
		Tags.Add( "trigger" );
	}
}
