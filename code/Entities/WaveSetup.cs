using Sandbox;
using System.Collections.Generic;
using SandboxEditor;
[Library( "td2_wave_setup" )]
[SupportsSolid]
[Description( "Sets up the wave" )]
[VisGroup( VisGroup.Logic )]
public class WaveSetup : Entity
{
	[Property( "WaveOrder" ), Description( "What order should this go" )]
	public int Wave_Order { get; set; }

	[Property( "SpawnCount" ), Description( "How many times should this spawn for that NPC" )]
	public int Spawn_Count { get; set; }

	public enum NPCEnum
	{
		Unspecified,
		//Normal NPCs
		Peasant,
	}

	[Property( "NPCToSpawn" ), Description( "What NPC should this spawn" )]
	public NPCEnum NPCs_To_Spawn { get; set; }

	[Property( "NPCSpawnRate" ), Description( "How fast should this NPC spawn" )]
	public double NPC_Spawn_Rate { get; set; }

	public override void Spawn()
	{
		base.Spawn();
	}
}
