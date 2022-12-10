using Sandbox;
using Editor;

[Library( "cd_tower_blocker" )]
[Title( "Tower Blocker" ), Description( "Prevents spawning of towers" )]
[Solid, RenderFields]
[HammerEntity]
public  class TowerBlocker : ModelEntity
{
	public override void Spawn()
	{
		base.Spawn();
		EnableDrawing = false;
		SetupPhysicsFromModel( PhysicsMotionType.Static );
		Tags.Add( "trigger" );
	}
}
