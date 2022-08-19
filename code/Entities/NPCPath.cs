using System;
using System.Linq;
using Sandbox;
using SandboxEditor;

[Library("cd_npc_path")]
[Title( "NPC Waypoint" ), Description( "Indicates a pathway for NPCs to follow" )]
[VisGroup( VisGroup.Logic )]
[HammerEntity]
public partial class NPCPath : Entity
{
	[Property(), Description( "The order of paths the NPC will go from start to end, " )]
	public int PathOrder { get; set; } = 1;
}

