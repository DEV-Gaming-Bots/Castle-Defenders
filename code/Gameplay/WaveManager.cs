using System.Linq;
using Components.popup;
using Sandbox;
using Sandbox.UI;

public  partial class CDGame
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
	public int LoopedTimes { get; set; }

	[Net]
	public TimeUntil TimeRemaining { get; private set; }

	[Net]
	public int CurWave { get; private set; }

	[Net]
	public int MaxWaves { get; private set; }

	private int _mapAttempts;

	private int _musicIndex;

	private bool _isBossWave;
	private bool _allowRestart;
	private bool _playInboundMusic;

	private MapVoteEntity _mapVote;

	public Entity[] MapCleanupFilter()
	{
		var list = All.Where( x => x is CDPawn || x.Parent is CDPawn ).ToArray();

		return list;
	}

	[Event.Tick.Server]
	public void TickGameplay()
	{
		if ( GameStatus == GameEnum.Idle )
			return;

		if ( Instance.Debug && Instance.DebugMode is DebugEnum.Gameplay or DebugEnum.All)
			Log.Info( TimeRemaining );

		if(TimeRemaining <= 10.0f && _playInboundMusic && WaveStatus == WaveEnum.Pre )
		{
			var waves = All.OfType<WaveSetup>().ToList().Where( x => x.WaveOrder == CurWave );

			foreach ( var wave in waves )
			{
				if ( wave.IsBossWave )
				{
					All.OfType<CDPawn>().ToList().ForEach( x => x.PlayMusic( To.Single( x ), "wave_inbound_boss" ) );
					break;
				}
			}

			_playInboundMusic = false;
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

				StartMapVote();
				return;

			case GameEnum.MapChange:
				Game.ChangeLevel( _mapVote.WinningMap );
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
		} );

		_mapVote = new MapVoteEntity();
	}

	public bool ShouldRestart()
	{
		if ( !_allowRestart )
			return false;

		if ( LoopedTimes > 1 )
			return false;

		if ( CurWave > (MaxWaves / 2) )
			return false;

		if ( _mapAttempts > 2 )
			return false;

		_mapAttempts++;

		return true;
	}

	public bool ShouldEndWave()
	{
		if ( TimeRemaining > -10.0f )
			return false;

		foreach ( var wave in All.OfType<WaveSetup>() )
		{
			if ( wave.CheckSpawnerCondition() )
				return false;
		}

		if ( All.OfType<BaseNPC>().Any() )
			return false;

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
		Game.ResetMap( MapCleanupFilter() );

		All.OfType<CompSetUp>().FirstOrDefault().SetUpCompGame();

		Instance.ActiveSuperTowerBlue = false;
		Instance.ActiveSuperTowerRed = false;

		_allowRestart = false;

		TimeRemaining = 10.0f;
		CurWave = 0;
		GameStatus = GameEnum.Starting;

		var checkWaves = 0;

		foreach ( var setter in All.OfType<WaveSetup>() )
		{
			if ( setter.WaveOrder > checkWaves )
				checkWaves++;
		}

		MaxWaves = checkWaves;
	}

	public void StartGame()
	{
		Game.ResetMap( MapCleanupFilter() );

		All.OfType<CDPawn>().ToList().ForEach( x => x.ResetPlayer() );

		Instance.ActiveSuperTowerBlue = false;

		All.OfType<CDPawn>().ToList().ForEach( x => x.EndMusic(To.Single(x), "" ) );

		_allowRestart = false;

		_mapAttempts = 0;
		TimeRemaining = 10.0f;
		CurWave = 0;
		GameStatus = GameEnum.Starting;

		var checkWaves = 0;

		foreach ( var setter in All.OfType<WaveSetup>() )
		{
			if( setter.WaveOrder > checkWaves)
				checkWaves++;
		}

		MaxWaves = checkWaves;
	}

	public void EndGame( WinningEnum winCondition = WinningEnum.Draw )
	{
		GameStatus = GameEnum.Post;
		TimeRemaining = 15.0f;

		All.OfType<BaseNPC>().ToList().ForEach( x => x.Delete() );

		if ( Competitive )
		{
			if(winCondition == WinningEnum.BlueWin)
			{
				WindowPopup.CreatePopUp( To.Everyone, "Blue team has won!", 5.0f, PopupVertical.Center, PopupHorizontal.Right );
				//ChatBox.AddChatEntry( To.Everyone, "GAME", "Blue team has won!" );
				All.OfType<CDPawn>().Where( x => x.CurTeam == CDPawn.TeamEnum.Blue ).ToList().ForEach( x => x.EndMusic( To.Single(x), "music_win" ) );
				All.OfType<CDPawn>().Where( x => x.CurTeam == CDPawn.TeamEnum.Red ).ToList().ForEach( x => x.EndMusic( To.Single( x ), "music_lost" ) );
			} 
			else if (winCondition == WinningEnum.RedWin)
			{
				WindowPopup.CreatePopUp( To.Everyone, "Red team has won!", 5.0f, PopupVertical.Center, PopupHorizontal.Right );
				//ChatBox.AddChatEntry( To.Everyone, "GAME", "Red team has won!" );
				All.OfType<CDPawn>().Where( x => x.CurTeam == CDPawn.TeamEnum.Red ).ToList().ForEach( x => x.EndMusic( To.Single( x ), "music_win" ) );
				All.OfType<CDPawn>().Where( x => x.CurTeam == CDPawn.TeamEnum.Blue ).ToList().ForEach( x => x.EndMusic( To.Single( x ), "music_lost" ) );
			}

		} 
		else
		{
			if( winCondition == WinningEnum.Win )
			{
				WindowPopup.CreatePopUp( To.Everyone, "You have successfully defended the castle!", 5.0f, PopupVertical.Center, PopupHorizontal.Right );
				//ChatBox.AddChatEntry( To.Everyone, "GAME", "You have successfully defended the castle!" );
				All.OfType<CDPawn>().ToList().ForEach( x => x.EndMusic( To.Single( x ), "music_win" ) );
			}
			else if (winCondition == WinningEnum.Lost )
			{
				WindowPopup.CreatePopUp( To.Everyone, "The evil forces have overtaken the castle!", 5.0f, PopupVertical.Center, PopupHorizontal.Right );
				//ChatBox.AddChatEntry( To.Everyone, "GAME", "The evil forces have overtaken the castle!" );
				All.OfType<CDPawn>().ToList().ForEach( x => x.EndMusic( To.Single( x ), "music_lost" ) );
				_allowRestart = true;
			}
		}
	}

	public void StartWave()
	{
		WaveStatus = WaveEnum.Active;

		var waveSetter = All.OfType<WaveSetup>().ToList().Where( x => x.WaveOrder == CurWave );

		foreach ( var setter in waveSetter )
		{
			setter.StartSpawning();

			if ( setter.IsBossWave )
				_isBossWave = true;
		}

		string music;
		_musicIndex = Game.Random.Int( 1, 2 );

		if ( _isBossWave )
			music = "boss_music_" + _musicIndex;
		else
			music = "wave_music_" + _musicIndex;

		All.OfType<CDPawn>().ToList().ForEach( x => x.EndMusic( To.Single( x ), music ) );
	}

	public void StartPreWave()
	{
		CurWave++;
		TimeRemaining = 10.0f;
		WaveStatus = WaveEnum.Pre;
	}

	public void PostWave()
	{
		if ( CurWave >= MaxWaves )
		{
			if ( !Instance.Competitive )
			{
				EndGame( WinningEnum.Win );
				return;
			} 
			else
			{
				CurWave = 0;
				LoopedTimes++;
			}
		}

		string endMusic;

		if ( _isBossWave )
			endMusic = $"boss_music_{_musicIndex}_end";
		else
			endMusic = $"wave_music_{_musicIndex}_end";

		All.OfType<CDPawn>().ToList().ForEach( x => x.EndMusic( To.Single(x), endMusic ) );

		_isBossWave = false;
		_playInboundMusic = true;

		WaveStatus = WaveEnum.Post;
		TimeRemaining = 10.0f;
	}

	public void ClearNPCs()
	{
		All.OfType<WaveSetup>().ToList().ToList().ForEach( x => x.StopSpawning() ); 
		All.OfType<BaseNPC>().ToList().ToList().ForEach( x => x.Delete() );
	}
}
