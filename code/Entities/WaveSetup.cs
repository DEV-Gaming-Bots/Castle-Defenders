﻿using Sandbox;
using SandboxEditor;

[Library( "cd_wave_setup" )]
[Title("Wave Setup"), Description( "Sets up the wave" )]
[VisGroup( VisGroup.Logic )]
[HammerEntity]
public class WaveSetup : Entity
{
	[Property( "WaveOrder" ), Description( "What order should this go, you can have multiple in a wave" )]
	public int Wave_Order { get; set; } = 1;

	[Property( "SpawnCount" ), Description( "How many times should this spawn for that NPC" )]
	public int Spawn_Count { get; set; } = 1;

	[Property( "IsBossWave" ), Description( "Is this a boss wave, you should only set only one on a wave" )]
	public bool IsBossWave { get; set; } = false;

	public enum NPCEnum
	{
		Unspecified,

		//Normal NPCs
		Peasant,
		Zombie,

		//Armoured NPCs
		Riot,
		Knight,

		//Splitter NPCs
		Husk,

		//Special
		Priest,

		//Advanced
		Ice,
		Magma,
		Void,

		//Airbone
		Spectre,

		//Bosses
		ZombieBoss,
		VoidBoss,
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

		if( NPCs_To_Spawn == NPCEnum.Unspecified )
			Log.Error( "One of the WaveSetup ents has unspecified NPCs, expect errors!" );
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

		if ( NPCs_To_Spawn == NPCEnum.Unspecified )
		{
			spawnToggle = false;
			return;
		}

		var npc = TypeLibrary.Create<BaseNPC>( NPCs_To_Spawn.ToString() );
		
		if( npc == null)
		{
			Log.Error( "This wave setup failed to spawn" );
			spawnToggle = false;
			return;
		}

		npc.Spawn();

		spawnCounter++;
		timeLastSpawn = 0;
	}

	public void StartSpawning()
	{
		timeLastSpawn = 0;
		spawnToggle = true;
	}
}
