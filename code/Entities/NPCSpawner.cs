using Sandbox;
using SandboxEditor;

[Library( "info_cd_npcportal" )]
[EditorModel( "models/npc_portal.vmdl" )]
[Title( "NPC Spawn Gate" ), Description( "Defines a point where NPCs can spawn" )]
[HammerEntity]
public class NPCSpawner : Entity
{
	public override void Spawn()
	{
		var portal = new ModelEntity();
		portal.SetModel( "models/npc_portal.vmdl" );
		portal.Position = Position;
		portal.Rotation = Rotation;
		portal.Spawn();
	}
}
