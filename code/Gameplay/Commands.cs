using System.Linq;
using Sandbox;

public sealed partial class CDGame
{
	[Net] public bool Debug { get; set; }
	public enum DebugEnum
	{
		Default,
		Tower,
		Gameplay,
		Path,
		All,
	}

	public DebugEnum DebugMode;

	[ConCmd.Admin( "cd_debug" )]
	public static void DebugToggle()
	{
		Instance.Debug = !Instance.Debug;

		Log.Info( "Debug: " + Instance.Debug );
	}

	[ConCmd.Admin( "cd_debugmode" )]
	public static void DebugModeSet(int mode)
	{
		Instance.DebugMode = mode switch
		{
			1 => DebugEnum.Default,
			2 => DebugEnum.Tower,
			3 => DebugEnum.Gameplay,
			4 => DebugEnum.Path,
			5 => DebugEnum.All,
		};

		Log.Info( "Debug Mode: " + Instance.DebugMode );
	}

	[ConCmd.Server("cd_money_give")]
	public static void GiveMoney(int amount, string targetName = "")
	{
		if ( !Instance.Debug )
		{
			Log.Error( "Debug is turned off" );
			return;
		}

		var player = ConsoleSystem.Caller.Pawn as CDPawn;

		if ( player == null )
			return;

		if( !string.IsNullOrEmpty(targetName) )
		{
			All.OfType<CDPawn>().ToList().FirstOrDefault( x => x.Client.Name.ToLower().Contains(targetName.ToLower()) )
				.AddCash( amount );
		}
		else
		{
			player.AddCash( amount );
		}
	}

	[ConCmd.Admin( "cd_diff_set" )]
	public static void SetDifficulty( int diffInt )
	{
		if ( !Instance.Debug )
		{
			Log.Error( "Debug is turned off" );
			return;
		}

		switch( diffInt )
		{
			case 1:
				Instance.Difficulty = DiffEnum.Easy;
				break;
			case 2:
				Instance.Difficulty = DiffEnum.Medium;
				break;
			case 3:
				Instance.Difficulty = DiffEnum.Hard;
				break;
			case 4:
				Instance.Difficulty = DiffEnum.Extreme;
				break;
			default:
				Log.Error( "Invalid setter for difficulty" );
				return;
		}

		Log.Info( "Updated difficulty to " + Instance.Difficulty );
	}

	[ConCmd.Server( "cd_npc_create" )]
	public static void SpawnNPC( string npcName, bool spawnOpposite = false)
	{
		if ( !Instance.Debug )
		{
			Log.Error( "Debug is turned off" );
			return;
		}

		var npc = new BaseNPC();

		if ( ResourceLibrary.TryGet( $"npcs/{npcName}.npc", out BaseNPCAsset asset ) )
			npc.UseAssetAndSpawn( asset );

		var spawnerPoint = All.OfType<NPCSpawner>().ToList();

		var blueSide = spawnerPoint.FirstOrDefault( x => x.AttackTeamSide == NPCSpawner.TeamEnum.Blue );
		var redSide = spawnerPoint.FirstOrDefault( x => x.AttackTeamSide == NPCSpawner.TeamEnum.Red );

		if ( !spawnOpposite && Instance.Competitive )
		{
			npc.Position = blueSide.Position;
			npc.Steer.Target = All.OfType<NPCPath>().FirstOrDefault( x => x.StartNode ).Position;
			npc.PathToFollow = BaseNPC.PathTeam.Blue;
			npc.CastleTarget = blueSide.FindCastle();
		} 
		else if ( spawnOpposite && Instance.Competitive )
		{
			npc.Position = redSide.Position;
			npc.Steer.Target = All.OfType<NPCPath>().FirstOrDefault( x => x.StartOpposingNode ).Position;
			npc.PathToFollow = BaseNPC.PathTeam.Red;
			npc.CastleTarget = redSide.FindCastle();
		} 
		else
		{
			npc.Position = blueSide.Position;
			npc.Steer.Target = All.OfType<NPCPath>().FirstOrDefault( x => x.StartNode ).Position;
			npc.PathToFollow = BaseNPC.PathTeam.Blue;
			npc.CastleTarget = blueSide.FindCastle();
		}
	}

	[ConCmd.Admin( "cd_force_start" )]
	public static void ForceStartGame()
	{
		if ( !Instance.Debug )
		{
			Log.Error( "Debug is turned off" );
			return;
		}

		if ( Instance.GameStatus != GameEnum.Idle )
		{
			Log.Error( "Game is not in Idle state" );
			return;
		}

		Instance.StartGame();
	}

	[ConCmd.Admin( "cd_force_stop" )]
	public static void ForceStopGame()
	{
		if ( !Instance.Debug )
		{
			Log.Error( "Debug is turned off" );
			return;
		}

		if ( Instance.GameStatus == GameEnum.Idle )
		{
			Log.Error( "Game is not in Idle state" );
			return;
		}

		Instance.GameStatus = GameEnum.Idle;
		Instance.WaveStatus = WaveEnum.Pre;
	}


	[ConCmd.Admin( "cd_force_restart" )]
	public static void ForceRestartGame()
	{
		if ( !Instance.Debug )
		{
			Log.Error( "Debug is turned off" );
			return;
		}

		if ( Instance.GameStatus == GameEnum.Idle )
		{
			Log.Error( "Game is in Idle state" );
			return;
		}

		Instance.StartGame();
	}

	[ConCmd.Admin( "cd_start_mapvote" )]
	public static void ForceMapVote()
	{
		if ( !Instance.Debug )
		{
			Log.Error( "Debug is turned off" );
			return;
		}

		Instance.StartMapVote();
	}

	[ConCmd.Admin( "cd_force_datareset" )]
	public static void ResetData()
	{
		if ( !Instance.Debug )
		{
			Log.Error( "Debug is turned off" );
			return;
		}


		var player = ConsoleSystem.Caller.Pawn as CDPawn;

		if ( player == null )
			return;

		player.NewPlayerStats();
	}

	[ConCmd.Admin( "cd_force_save" )]
	public static void SaveData()
	{
		if ( !Instance.Debug )
		{
			Log.Error( "Debug is turned off" );
			return;
		}

		var player = ConsoleSystem.Caller.Pawn as CDPawn;

		if ( player == null )
			return;

		Instance.SaveData( player );
	}

	[ConCmd.Admin( "cd_force_saveall" )]
	public static void SaveAllData()
	{
		if ( !Instance.Debug )
			return;

		foreach ( var cl in Client.All )
		{
			if ( cl.Pawn is CDPawn player )
				Instance.SaveData( player );
		}
	}

	[ConCmd.Admin( "cd_force_loaddata" )]
	public static void LoadDataCMD()
	{
		if ( !Instance.Debug )
		{
			Log.Error( "Debug is turned off" );
			return;
		}

		Instance.LoadSave( ConsoleSystem.Caller );
	}

	[ConCmd.Server( "cd_set_towerslot" )]
	public static void SelectTower(int slot, string name)
	{
		var player = ConsoleSystem.Caller.Pawn as CDPawn;
		
		if ( player == null )
			return;

		if ( name.ToLower().Contains( "hands" ) )
		{
			player.ChangeSlot( "Hands", player.TowerSlots.Count );
			return;
		}

		if ( TypeLibrary.GetDescription<Entity>( name ) == null )
		{
			Log.Error( "Invalid replacement slot name" );
			return;
		}

		player.TowerSlots[slot] = name;
		player.ChangeSlot( name, slot );
	}

	[ConCmd.Server( "cd_get_towerslots" )]
	public static void GetSlots()
	{
		var player = ConsoleSystem.Caller.Pawn as CDPawn;
		if ( player == null )
			return;

		var slotNum = 1;

		foreach ( var item in player.TowerSlots )
		{
			player.UpdateSlots( To.Single( player ), item.Value, item.Key + 1);
			slotNum++;
		}

		player.UpdateSlots( To.Single( player ), "Hands", 0 );
	}

	[ConCmd.Server("cd_wave_set")]
	public static void SetWave(int wave)
	{
		if ( !Instance.Debug )
		{
			Log.Error( "Debug is turned off" );
			return;
		}

		Instance.ClearNPCs();
		Instance.PostWave();
		Instance.CurWave = Instance.CurWave.Clamp(wave - 1, Instance.MaxWaves);
	}

	[ConCmd.Server( "cd_wave_restart" )]
	public static void RestartWave()
	{
		if ( !Instance.Debug )
		{
			Log.Error( "Debug is turned off" );
			return;
		}

		Instance.ClearNPCs();
		Instance.PostWave();
		Instance.CurWave -= 1;
	}
}
