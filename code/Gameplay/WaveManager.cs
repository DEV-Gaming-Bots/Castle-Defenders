using System;
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
	public TimeUntil TimeRemaining { get; protected set; }

	[Net]
	public int CurWave { get; protected set; }

	[Net]
	public int MaxWaves { get; protected set; }

	int mapAttempts;

	bool allowRestart;

	MapVoteEntity mapVote;

	[Event.Tick.Server]
	public void TickGameplay()
	{
		if ( GameStatus == GameEnum.Idle )
			return;

		if ( Instance.Debug && (Instance.DebugMode == DebugEnum.Gameplay || Instance.DebugMode == DebugEnum.All))
			Log.Info( TimeRemaining );

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
		TimeRemaining = 32.0f;

		GameStatus = GameEnum.MapChange;
		WaveStatus = WaveEnum.MapChange;

		mapVote = new MapVoteEntity();
	}

	public bool ShouldRestart()
	{
		if ( !allowRestart )
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
			if ( All.OfType<BaseNPC>().Count() > 0 )
				return false;

			if ( wave.CheckSpawnerCondition() == true )
				return false;
		}

		return true;
	}

	public void StartGame()
	{
		Map.Reset(DefaultCleanupFilter);

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

		All.OfType<CDPawn>().ToList().ForEach( x =>
		{
			x.SelectedTower.Delete();
			x.SelectedTower = null;
		} );

		MaxWaves = checkWaves;
	}

	public void EndGame( WinningEnum winCondition = WinningEnum.Draw )
	{
		GameStatus = GameEnum.Post;
		TimeRemaining = 15.0f;

		if ( Competitive )
		{
			//TODO, Competitive endgame situations for teams
		} else
		{
			if( winCondition == WinningEnum.Win )
			{

			}
			else if (winCondition == WinningEnum.Lost )
			{
				allowRestart = true;
			}

		}

	}

	public void StartWave()
	{
		WaveStatus = WaveEnum.Active;

		var waveSetter = All.OfType<WaveSetup>().ToList().Where( x => x.Wave_Order == CurWave );

		foreach ( var setter in waveSetter )
			setter.StartSpawning();
	}

	public void StartPreWave()
	{
		CurWave++;
		TimeRemaining = 30.0f;
		WaveStatus = WaveEnum.Pre;
	}

	public void PostWave()
	{
		if ( CurWave >= MaxWaves )
		{
			EndGame();
			return;
		}

		WaveStatus = WaveEnum.Post;
		TimeRemaining = 10.0f;
	}

}
