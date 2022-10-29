using Sandbox;
using SandboxEditor;
using System.Linq;

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

	[Property( "NPCSpawnDelay" ), Description( "After how many seconds should this NPC start spawning" )]
	public double NPC_Spawn_Delay { get; set; } = 0.0f;
	
	[Property( "NPCPathPriority" ), Description( "What direction this NPC will take when the path splits" )]
	public BaseNPC.PathPriority NPC_Path_Priority { get; set; } = BaseNPC.PathPriority.Random;


	bool spawnToggle;
	TimeSince timeLastSpawn;
	TimeSince timeSinceWaveStart;
	int spawnCounter;
	bool spawnOpposite;

	public bool CheckSpawnerCondition()
	{
		if ( spawnCounter - Spawn_Count == 0 )
			return false;

		return spawnToggle;
	}

	public override void Spawn()
	{
		base.Spawn();
		spawnToggle = false;
		spawnCounter = 0;
		spawnOpposite = false;

		if( NPCs_To_Spawn == NPCEnum.Unspecified )
			Log.Error( "One of the WaveSetup ents has unspecified NPCs, expect errors!" );
	}

	[Event.Tick.Server]
	public void TickSpawning()
	{
		if ( !spawnToggle )
			return;

		if ( timeSinceWaveStart < NPC_Spawn_Delay )
			return;

		if ( timeLastSpawn < NPC_Spawn_Rate )
			return;

		if ( spawnCounter >= Spawn_Count )
		{
			spawnToggle = false;
			return;
		}

		if ( NPCs_To_Spawn == NPCEnum.Unspecified )
		{
			spawnToggle = false;
			return;
		}

		var npc = TypeLibrary.Create<BaseNPC>( NPCs_To_Spawn.ToString() );
		
		if( npc == null )
		{
			Log.Error( "This wave setup failed to spawn" );
			spawnToggle = false;
			return;
		}

		npc.Spawn();
		
		npc.nextPathPriority = NPC_Path_Priority;

		if ( CDGame.Instance.Competitive )
		{
			var spawnerpoint = All.OfType<NPCSpawner>().ToList();

			var blueSide = spawnerpoint.Where( x => x.AttackTeamSide == NPCSpawner.TeamEnum.Blue ).First();
			var redSide = spawnerpoint.Where( x => x.AttackTeamSide == NPCSpawner.TeamEnum.Red ).First();

			if(spawnOpposite)
			{
				if ( blueSide != null )
				{
					npc.Position = blueSide.Position;
					npc.Steer.Target = All.OfType<NPCPath>().Where( x => x.StartNode ).FirstOrDefault().Position;
					npc.PathToFollow = BaseNPC.PathTeam.Blue;
					npc.CastleTarget = blueSide.FindCastle();
				}
			
			} 
			else
			{
				if ( redSide != null )
				{
					npc.Position = redSide.Position;
					npc.Steer.Target = All.OfType<NPCPath>().Where( x => x.StartOpposingNode ).FirstOrDefault().Position;
					npc.PathToFollow = BaseNPC.PathTeam.Red;
					npc.CastleTarget = redSide.FindCastle();
				}
			}

			spawnOpposite = !spawnOpposite;
		} 
		else
		{
			var spawnerpoint = All.OfType<NPCSpawner>().ToList();

			var blueSide = spawnerpoint.Where( x => x.AttackTeamSide == NPCSpawner.TeamEnum.Blue ).First();
			npc.CastleTarget = blueSide.FindCastle();
			npc.Steer.Target = All.OfType<NPCPath>().Where( x => x.StartNode ).FirstOrDefault().Position;
			npc.PathToFollow = BaseNPC.PathTeam.Blue;
			npc.Position = blueSide.Position;
		}

		spawnCounter++;
		timeLastSpawn = 0;
	}

	public void StartSpawning()
	{
		if ( CDGame.Instance.Competitive )
		{
			Spawn_Count *= 2;
			NPC_Spawn_Rate /= 2;
		}

		spawnCounter = 0;
		timeLastSpawn = 0;
		timeSinceWaveStart = 0;
		spawnToggle = true;
	}
}
