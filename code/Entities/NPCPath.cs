using System.Linq;
using Sandbox;
using Editor;

[Library("cd_npc_path")]
[Title( "NPC Path Nodes" ), Description( "Indicates a pathway for NPCs to follow" )]
[HammerEntity]
public  class NPCPath : ModelEntity
{
	[Property, Description("Path to the next path node")]
	public EntityTarget NextPath { get; set; }

	[Property, Description( "The split path node, allows npcs to choose different directions" )]
	public EntityTarget SplitPathOrder { get; set; }

	[Property, Description( "Is this node the starting path? required for npcs to navigate" )]
	public bool StartNode { get; set; } = false;

	[Property, Description( "Like Start node except for the opposing side, only enable this on the red side" )]
	public bool StartOpposingNode { get; set; } = false;

	[Property, Description( "Should the last node teleport NPC to this node instantly" )]
	public bool TeleportingNode { get; set; } = false;

	[Property, Description( "A multiplier to the movement speed while approaching this node" )]
	public float NodeSpeed { get; set; } = 1.0f;

	public Entity NextNode;
	public Entity NextSplitNode;

	private bool _takeNormalPath = false;

	public override void Spawn()
	{
		base.Spawn();
		NextNode = FindNormalPath();
		NextSplitNode = FindSplitPath();
	}

	public Entity FindNextPath( BaseNPC.PathPriority pathPriority )
	{
		var normalPath = FindNormalPath();
		var splitPath = FindSplitPath();
		
		if ( normalPath == null )
			return null;

		if ( splitPath == null || pathPriority == BaseNPC.PathPriority.Normal )
		{
			return normalPath;
		}

		if ( pathPriority == BaseNPC.PathPriority.Random )
		{
			switch ( Game.Random.Int( 1, 2 ) )
			{
				case 1:
					return normalPath;
				case 2:
					return splitPath;
			}
		}
		
		if ( pathPriority == BaseNPC.PathPriority.Split )
		{
			return splitPath;
		}
		
		if ( pathPriority == BaseNPC.PathPriority.Alternate )
		{
			_takeNormalPath = !_takeNormalPath;
			
			if ( _takeNormalPath )
			{
				return normalPath;
			}
			
			return splitPath;
		}

		return normalPath;
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

		var splitNode = SplitPathOrder.GetTargets().FirstOrDefault();

		return splitNode;
	}
}

