using System;
using Sandbox;
using SandboxEditor;

[Library("cd_npc_path")]
[Title( "NPC Waypoint" ), Description( "Indicates a pathway for NPCs to follow" )]
[VisGroup( VisGroup.Logic )]
[HammerEntity]
public partial class NPCPath : Entity
{
	[Property( "Path_Order" ), Description( "The order of paths the NPC will go from start to end" )]
	public int Path_Order { get; set; }

	public override void Spawn()
	{

	}
}

