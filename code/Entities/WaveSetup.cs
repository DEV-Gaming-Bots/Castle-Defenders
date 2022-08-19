using Sandbox;
using System.Collections.Generic;
using SandboxEditor;

[Library( "cd_wave_setup" )]
[Title("Wave Setup"), Description( "Sets up the wave" )]
[VisGroup( VisGroup.Logic )]
[HammerEntity]
public class WaveSetup : Entity
{
	[Property( "WaveOrder" ), Description( "What order should this go" )]
	public int Wave_Order { get; set; } = 1;

	[Property( "SpawnCount" ), Description( "How many times should this spawn for that NPC" )]
	public int Spawn_Count { get; set; } = 1;

	public enum NPCEnum
	{
		Unspecified,
		//Normal NPCs
		Peasant,
	}

	[Property( "NPCToSpawn" ), Description( "What NPC should this spawn" )]
	public NPCEnum NPCs_To_Spawn { get; set; } = NPCEnum.Unspecified;

	[Property( "NPCSpawnRate" ), Description( "How fast should this NPC spawn" )]
	public double NPC_Spawn_Rate { get; set; } = 1.0f;

	bool spawnToggle;
	TimeSince timeLastSpawn;
	int spawnCounter;

	public bool CheckSpawnerCondition()
	{
		return spawnToggle;
	}

	public override void Spawn()
	{
		base.Spawn();
		spawnToggle = false;
		spawnCounter = 0;
	}

	[Event.Tick.Server]
	public void TickSpawning()
	{
		if ( !spawnToggle )
			return;

		if ( timeLastSpawn < NPC_Spawn_Rate )
			return;

		if ( spawnCounter >= Spawn_Count )
		{
			spawnToggle = false;
			spawnCounter = 0;
			return;
		}

		TypeLibrary.Create<BaseNPC>( NPCs_To_Spawn.ToString() ).Spawn();
		spawnCounter++;
		timeLastSpawn = 0;
	}

	public void StartSpawning()
	{
		timeLastSpawn = 0;
		spawnToggle = true;
	}
}
