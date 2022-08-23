using System;
using System.Linq;
using Sandbox;
using SandboxEditor;

[Library("cd_npc_path")]
[Title( "NPC Path Nodes" ), Description( "Indicates a pathway for NPCs to follow" )]
[HammerEntity]
public partial class NPCPath : Entity
{
	[Property, Description("Path to the next path node")]
	public EntityTarget NextPath { get; set; }

	[Property, Description( "The split path node, allows npcs to choose different directions" )]
	public EntityTarget SplitPathOrder { get; set; }

	[Property, Description( "Is this node the starting path? required for npcs to navigate" )]
	public bool StartNode { get; set; } = false;

	public Entity NextNode;
	public Entity NextSplitNode;

	public override void Spawn()
	{
		base.Spawn();
	}

	public Entity FindNormalPath()
	{
		NextNode = NextPath.GetTargets( null ).FirstOrDefault();

		return NextNode;
	}

	public Entity FindSplitPath()
	{
		if ( SplitPathOrder.IsValid() )
			NextSplitNode = SplitPathOrder.GetTargets( null ).FirstOrDefault();

		return NextSplitNode;
	}
}

