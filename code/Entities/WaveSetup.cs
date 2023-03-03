using Sandbox;
using Sandbox.UI;
using Editor;
using System.Linq;
using Components.popup;

[Library( "cd_wave_setup" )]
[Title("Wave Setup"), Description( "Sets up the wave" )]
[VisGroup( VisGroup.Logic )]
[HammerEntity]
public  class WaveSetup : Entity
{
	[Property( "WaveOrder" ), Description( "What order should this go, you can have multiple in a wave" )]
	public int WaveOrder { get; set; } = 1;

	[Property( "SpawnCount" ), Description( "How many times should this spawn for that NPC" )]
	public int SpawnCount { get; set; } = 1;

	[Property( "IsBossWave" ), Description( "Is this a boss wave, you should set only one on a wave" )]
	public bool IsBossWave { get; set; } = false;

	[Property, Description("A message that will be displayed in the chat")]
	public string NoteText { get; set; } = "";

	[Property( "NPCToSpawn" ), Title( "Spawning NPC" ), Description( "What NPC should this spawn" )]
	public BaseNPCAsset NPCsToSpawn { get; set; }

	[Property( "NPCSpawnRate" ), Title("Spawn Rate"), Description( "How fast should this NPC spawn" )]
	public double NPCSpawnRate { get; set; } = 1.0f;

	[Property( "NPCSpawnDelay" ), Title( "Spawn Delay" ), Description( "After how many seconds should this NPC start spawning" )]
	public double NPCSpawnDelay { get; set; } = 0.0f;

	[Property( "NPCPathPriority" ), Title( "Path Priority" ), Description( "What direction this NPC will take when the path splits" )]
	public BaseNPC.PathPriority NPCPathPriority { get; set; } = BaseNPC.PathPriority.Random;

	private bool _spawnToggle;
	private TimeSince _timeLastSpawn;
	private TimeSince _timeSinceWaveStart;
	private int _spawnCounter;
	private bool _spawnOpposite;

	public bool CheckSpawnerCondition()
	{
		if ( _spawnCounter - SpawnCount == 0 )
			return false;

		return _spawnToggle;
	}

	public override void Spawn()
	{
		base.Spawn();
		_spawnToggle = false;
		_spawnCounter = 0;
		_spawnOpposite = false;

		if ( NPCsToSpawn is null )
			Log.Error( "One of the WaveSetup ents has null NPCs, expect errors!" );
	}

	int spawnOrder;

	[Event.Tick.Server]
	public void TickSpawning()
	{
		if ( !_spawnToggle )
			return;

		if ( _timeSinceWaveStart < NPCSpawnDelay )
			return;

		if ( _timeLastSpawn < NPCSpawnRate )
			return;

		if ( _spawnCounter >= SpawnCount )
		{
			_spawnToggle = false;
			return;
		}

		if ( NPCsToSpawn is null )
		{
			_spawnToggle = false;
			return;
		}


		var npc = new BaseNPC();
		npc.UseAssetAndSpawn( NPCsToSpawn );
		npc.Order = spawnOrder;

		spawnOrder++;
		if ( npc == null )
		{
			Log.Error( "This wave setup failed to spawn" );
			_spawnToggle = false;
			return;
		}
		
		npc.NextPathPriority = NPCPathPriority;

		if ( CDGame.Instance.Competitive )
		{
			var spawnerPoint = All.OfType<NPCSpawner>().ToList();

			var blueSide = spawnerPoint.First( x => x.AttackTeamSide == NPCSpawner.TeamEnum.Blue );
			var redSide = spawnerPoint.First( x => x.AttackTeamSide == NPCSpawner.TeamEnum.Red );

			if(_spawnOpposite)
			{
				if ( blueSide != null )
				{
					npc.Position = blueSide.Position;
					npc.Steer.Target = All.OfType<NPCPath>().FirstOrDefault( x => x.StartNode ).Position;
					npc.PathToFollow = BaseNPC.PathTeam.Blue;
					npc.CastleTarget = blueSide.FindCastle();
				}
			} 
			else
			{
				if ( redSide != null )
				{
					npc.Position = redSide.Position;
					npc.Steer.Target = All.OfType<NPCPath>().FirstOrDefault( x => x.StartOpposingNode ).Position;
					npc.PathToFollow = BaseNPC.PathTeam.Red;
					npc.CastleTarget = redSide.FindCastle();
				}
			}

			_spawnOpposite = !_spawnOpposite;
		} 
		else
		{
			var spawnerPoint = All.OfType<NPCSpawner>().ToList();

			var blueSide = spawnerPoint.First( x => x.AttackTeamSide == NPCSpawner.TeamEnum.Blue );
			npc.CastleTarget = blueSide.FindCastle();
			npc.Steer.Target = All.OfType<NPCPath>().FirstOrDefault( x => x.StartNode ).Position;
			npc.PathToFollow = BaseNPC.PathTeam.Blue;
			npc.Position = blueSide.Position;
		}

		_spawnCounter++;
		_timeLastSpawn = 0;
	}

	public void StartSpawning()
	{
		if ( CDGame.Instance.Competitive )
		{
			SpawnCount *= 2;
			NPCSpawnRate /= 2;
		}

		if ( !string.IsNullOrEmpty( NoteText ) )
			WindowPopup.CreatePopUp( To.Everyone, NoteText, 5.0f, PopupVertical.Center, PopupHorizontal.Left );

		_spawnCounter = 0;
		_timeLastSpawn = 0;
		_timeSinceWaveStart = 0;
		_spawnToggle = true;
		spawnOrder = 0;
	}

	public void StopSpawning()
	{
		_spawnToggle = false;
	}
}
