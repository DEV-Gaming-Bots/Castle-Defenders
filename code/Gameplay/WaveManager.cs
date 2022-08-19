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

	public GameEnum GameStatus;
	public WaveEnum WaveStatus;
	public DiffVariants DifficultyVariant;

	public DiffEnum Difficulty;

	[Net]
	public RealTimeUntil TimeRemaining { get; protected set; }

	[Net]
	public int CurWave { get; protected set; }

	[Net]
	public int MaxWaves { get; protected set; }

	int mapAttempts;

	[Event.Tick.Server]
	public void TickGameplay()
	{
		if ( GameStatus == GameEnum.Idle )
			return;

		if( Instance.Debug && (Instance.DebugMode == DebugEnum.Gameplay || Instance.DebugMode == DebugEnum.All))
			Log.Info( TimeRemaining );

		if( TimeRemaining <= 0.0f )
		{
			switch ( GameStatus )
			{
				case GameEnum.Starting:
					GameStatus = GameEnum.Active;
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
	}

	public void StartMapVote()
	{
		GameStatus = GameEnum.MapChange;
		WaveStatus = WaveEnum.MapChange;

		_ = new MapVoteEntity();
	}

	public bool ShouldRestart()
	{
		if ( CurWave < (MaxWaves / 2) )
			return false;

		if ( mapAttempts < 3 )
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
		mapAttempts = 0;
		TimeRemaining = 10.0f;
		CurWave = 0;
		GameStatus = GameEnum.Starting;

		MaxWaves = All.OfType<WaveSetup>().Count();
	}

	public void StartWave()
	{
		WaveStatus = WaveEnum.Active;

		All.OfType<WaveSetup>().FirstOrDefault( x => x.Wave_Order == CurWave ).StartSpawning();
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
			GameStatus = GameEnum.Post;
			TimeRemaining = 5.0f;
			return;
		}

		WaveStatus = WaveEnum.Post;
		TimeRemaining = 10.0f;
	}

}
