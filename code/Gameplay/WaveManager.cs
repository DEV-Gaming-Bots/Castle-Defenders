using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Sandbox;
public partial class CDGame
{
	//Basic Gameplay and wave statuses
	public enum GameEnum
	{
		Idle,
		Starting,
		Active,
		Post,
		MapChange
	}

	public enum WaveEnum
	{
		Pre,
		Active,
		Post,
		MapChange,
	}

	//Difficulty
	public enum DiffEnum
	{
		Easy,
		Medium,
		Hard,
		Extreme
	}

	//To add towards difficulty, players can opt to choose a difficulty variant
	public enum DiffVariants
	{
		None,
		//TODO: Think of difficulty variants
	}

	public enum WinningEnum
	{
		Draw,
		Lost,
		Win,
		BlueWin,
		RedWin,
	}

	[Net]
	public GameEnum GameStatus { get; protected set; }
	
	[Net]
	public WaveEnum WaveStatus { get; protected set; }

	public DiffVariants DifficultyVariant;

	public DiffEnum Difficulty;
	
	[Net]
	public bool LoopGame { get; set; }

	[Net]
	public int LoopedTimes { get; set; }

	[Net]
	public TimeUntil TimeRemaining { get; protected set; }

	[Net]
	public int CurWave { get; protected set; }

	[Net]
	public int MaxWaves { get; protected set; }

	int mapAttempts;

	int musicIndex;

	bool isBossWave;
	bool allowRestart;
	bool playInboundMusic = false;

	MapVoteEntity mapVote;

	[Event.Tick.Server]
	public void TickGameplay()
	{
		if ( GameStatus == GameEnum.Idle )
			return;

		if ( Instance.Debug && (Instance.DebugMode == DebugEnum.Gameplay || Instance.DebugMode == DebugEnum.All))
			Log.Info( TimeRemaining );

		if(TimeRemaining <= 10.0f && playInboundMusic && WaveStatus == WaveEnum.Pre )
		{
			var waves = All.OfType<WaveSetup>().ToList().Where( x => x.Wave_Order == CurWave );

			foreach ( var wave in waves )
			{
				if ( wave.IsBossWave )
				{
					All.OfType<CDPawn>().ToList().ForEach( x => x.PlayMusic( To.Single( x ), "wave_inbound_boss" ) );
					break;
				}
			}

			playInboundMusic = false;
		}


		if ( TimeRemaining > 0.0f )
			return;

		switch ( GameStatus )
		{
			case GameEnum.Starting:
				GameStatus = GameEnum.Active;
				All.OfType<CDPawn>().ToList().ForEach( x => x.SetUpPlayer() );
				StartPreWave();
				return;
			case GameEnum.Post:
				
				if ( ShouldRestart() )
				{
					StartGame();
					return;
				}

				if ( LoopGame )
				{
					LoopRestartGame();
					return;
				}

				StartMapVote();
				return;

			case GameEnum.MapChange:
				Global.ChangeLevel( mapVote.WinningMap );
				break;
		}

		switch ( WaveStatus )
		{
			case WaveEnum.Pre:
				StartWave();
				break;

			case WaveEnum.Active:
				if ( ShouldEndWave() )
					PostWave();
				break;

			case WaveEnum.Post:
				StartPreWave();
				break;
		}
	}

	public void StartMapVote()
	{
		TimeRemaining = 22.0f;

		GameStatus = GameEnum.MapChange;
		WaveStatus = WaveEnum.MapChange;

		All.OfType<CDPawn>().ToList().ForEach( x =>
		{
			x.PreviewTower?.Delete();
			x.PreviewTower = null;

			x.TowerInHand?.Delete();
			x.TowerInHand = null;
		} );

		mapVote = new MapVoteEntity();
	}

	public bool ShouldRestart()
	{
		if ( !allowRestart )
			return false;

		if ( LoopedTimes > 1 )
			return false;

		if ( CurWave > (MaxWaves / 2) )
			return false;

		if ( mapAttempts > 2 )
			return false;

		mapAttempts++;

		return true;
	}

	public bool ShouldEndWave()
	{
		foreach ( var wave in All.OfType<WaveSetup>().ToList() )
		{
			if ( wave.CheckSpawnerCondition() == true )
				return false;
		}

		foreach ( var npc in All.OfType<BaseNPC>().ToList())
		{
			if ( npc.IsValid() )
				return false;
		}

		return true;
	}

	public void LoopRestartGame()
	{
		All.OfType<CDPawn>().ToList().ForEach( x => x.EndMusic( To.Single( x ), "" ) );

		LoopedTimes++;
		TimeRemaining = 10.0f;
		CurWave = 0;

		GameStatus = GameEnum.Starting;
	}

	public void StartCompGame()
	{
		Map.Reset( DefaultCleanupFilter );

		Instance.ActiveSuperTowerBlue = false;
		Instance.ActiveSuperTowerRed = false;

		allowRestart = false;

		TimeRemaining = 10.0f;
		CurWave = 0;
		GameStatus = GameEnum.Starting;

		int checkWaves = 0;

		foreach ( var setter in All.OfType<WaveSetup>() )
		{
			if ( setter.Wave_Order > checkWaves )
				checkWaves++;
		}

		MaxWaves = checkWaves;
	}
	public void StartGame()
	{
		All.OfType<CDPawn>().ToList().ForEach( x => x.DestroyPreview( To.Single( x ) ) );

		Map.Reset(DefaultCleanupFilter);

		Instance.ActiveSuperTowerBlue = false;

		All.OfType<CDPawn>().ToList().ForEach( x => x.EndMusic(To.Single(x), "" ) );

		allowRestart = false;

		mapAttempts = 0;
		TimeRemaining = 10.0f;
		CurWave = 0;
		GameStatus = GameEnum.Starting;

		int checkWaves = 0;

		foreach ( var setter in All.OfType<WaveSetup>() )
		{
			if( setter.Wave_Order > checkWaves)
				checkWaves++;
		}

		MaxWaves = checkWaves;
	}

	public void EndGame( WinningEnum winCondition = WinningEnum.Draw )
	{
		GameStatus = GameEnum.Post;
		TimeRemaining = 15.0f;

		if ( Competitive )
		{
			//TODO, Competitive endgame situations for teams

		} 
		else
		{
			if( winCondition == WinningEnum.Win )
			{
				All.OfType<CDPawn>().ToList().ForEach( x => x.EndMusic( To.Single( x ), "music_win" ) );
			}
			else if (winCondition == WinningEnum.Lost )
			{
				All.OfType<CDPawn>().ToList().ForEach( x => x.EndMusic( To.Single( x ), "music_lost" ) );
				allowRestart = true;
			}
		}
	}

	public void StartWave()
	{
		WaveStatus = WaveEnum.Active;

		var waveSetter = All.OfType<WaveSetup>().ToList().Where( x => x.Wave_Order == CurWave );

		foreach ( var setter in waveSetter )
		{
			setter.StartSpawning();

			if ( setter.IsBossWave )
				isBossWave = true;
		}

		string music = "";
		musicIndex = Rand.Int( 1, 3 );

		if ( isBossWave )
			music = "boss_music_" + musicIndex;
		else
			music = "wave_music_" + musicIndex;

		All.OfType<CDPawn>().ToList().ForEach( x => x.EndMusic( To.Single( x ), music ) );
	}

	public void StartPreWave()
	{
		CurWave++;
		TimeRemaining = 20.0f;
		WaveStatus = WaveEnum.Pre;
	}

	public void PostWave()
	{
		if ( CurWave >= MaxWaves )
		{
			EndGame(WinningEnum.Win);
			return;
		}

		string endMusic = "";

		if ( isBossWave )
			endMusic = $"boss_music_{musicIndex}_end";
		else
			endMusic = $"wave_music_{musicIndex}_end";

		All.OfType<CDPawn>().ToList().ForEach( x => x.EndMusic( To.Single(x), endMusic ) );

		isBossWave = false;
		playInboundMusic = true;

		WaveStatus = WaveEnum.Post;
		TimeRemaining = 10.0f;
	}

}
