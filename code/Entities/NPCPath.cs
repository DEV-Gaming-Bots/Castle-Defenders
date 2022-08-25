using System;
using System.Linq;
using Sandbox;
using SandboxEditor;

[Library("cd_npc_path")]
[Title( "NPC Path Nodes" ), Description( "Indicates a pathway for NPCs to follow" )]
[HammerEntity]
public partial class NPCPath : ModelEntity
{
	[Property, Description("Path to the next path node")]
	public EntityTarget NextPath { get; set; }

	[Property, Description( "The split path node, allows npcs to choose different directions" )]
	public EntityTarget SplitPathOrder { get; set; }

	[Property, Description( "Is this node the starting path? required for npcs to navigate" )]
	public bool StartNode { get; set; } = false;

	[Property, Description( "Like Start node except for the opposing side, only enable this on the red side" )]
	public bool StartOpposingNode { get; set; } = false;

	public Entity NextNode;
	public Entity NextSplitNode;

	public override void Spawn()
	{
		base.Spawn();
		NextNode = FindNormalPath();
		NextSplitNode = FindSplitPath();
	}

	public Entity FindNormalPath()
	{
		var nextNode = NextPath.GetTargets( null ).FirstOrDefault();

		return nextNode;
	}

	public Entity FindSplitPath()
	{
		if ( !SplitPathOrder.IsValid() )
			return null;

		var splitNode = SplitPathOrder.GetTargets( null ).FirstOrDefault();

		return splitNode;
	}
}

