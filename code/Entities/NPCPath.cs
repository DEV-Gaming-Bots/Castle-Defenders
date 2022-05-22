using System;
using Sandbox;
using SandboxEditor;

[Library("td2_npc_path")]
[Description( "Indicates a pathway for NPCs to follow" )]
[VisGroup( VisGroup.Logic )]
public partial class NPCPath : Entity
{
	[Property( "Path_Order" ), Description( "The order of paths the NPC will go from start to end" )]
	public int Path_Order { get; set; }

	public override void Spawn()
	{

	}
}

