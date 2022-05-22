using Sandbox;
using System.Collections.Generic;

[Library( "info_td2_npcportal" )]
[EditorModel( "models/npc_portal.vmdl" )]
[Description( "Defines a point where NPCs can spawn" )]
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
