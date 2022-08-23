using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public partial class CDGame
{
	public bool Debug;
	public enum DebugEnum
	{
		Default,
		Tower,
		Gameplay,
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
		switch(mode)
		{
			case 1:
				Instance.DebugMode = DebugEnum.Default;
				break;
			case 2:
				Instance.DebugMode = DebugEnum.Tower;
				break;
			case 3:
				Instance.DebugMode = DebugEnum.Gameplay;
				break;
			case 4:
				Instance.DebugMode = DebugEnum.All;
				break;
		}

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
			All.OfType<CDPawn>().ToList().Where( x => x.Client.Name.ToLower().Contains(targetName.ToLower()) ).FirstOrDefault()
				.AddCash( amount );
		}
		else
		{
			player.AddCash( amount );
		}
	}

	[ConCmd.Server( "cd_diff_set" )]
	public static void SetDifficulty( string diffName )
	{
		if ( !Instance.Debug )
		{
			Log.Error( "Debug is turned off" );
			return;
		}

		switch(diffName.ToLower())
		{
			case "easy":
				Instance.Difficulty = DiffEnum.Easy;
				break;
			case "medium":
				Instance.Difficulty = DiffEnum.Medium;
				break;
			case "hard":
				Instance.Difficulty = DiffEnum.Hard;
				break;
			case "extreme":
				Instance.Difficulty = DiffEnum.Extreme;
				break;
			default:
				Log.Error( "Invalid setter for difficulty" );
				return;
		}

		Log.Info( "Updated difficulty to " + Instance.Difficulty );
	}

	[ConCmd.Server( "cd_npc_create" )]
	public static void SpawnNPC( string npcName )
	{
		if ( !Instance.Debug )
		{
			Log.Error( "Debug is turned off" );
			return;
		}

		var npc = TypeLibrary.Create<BaseNPC>( npcName );

		if ( npc == null )
		{
			Log.Error( "Invalid npc name" );
			return;
		}

		npc.Spawn();
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

	[ConCmd.Server( "cd_tower_select" )]
	public static void SelectTower()
	{

	}
}
