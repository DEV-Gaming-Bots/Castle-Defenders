using System;
using Sandbox;
using Sandbox.UI;
using CastleDefenders.UI.Components;

public  class StatusGame : Panel
{
	public TimeSince TimerElapsed;
	//public GameStats.GameInfoPanel gameInfo = new();
	public GameStats.GameStatsPanel GameStats = new();
	public GameStats.GameInfoPanel.ExtraGameInfo Loop = new();
	public GameStats.GameInfoPanel.ExtraGameInfo Waves = new();
	public StatusGame()
	{
		StyleSheet.Load( "UI/StatusGame.scss" );

		//AddChild( gameInfo );
		AddChild( GameStats );

		//gameInfo.TextTimer = "bruh";
		//gameInfo.TextRounds = "lmao";
	}

	public override void Tick()
	{
		base.Tick();

		if ( CDGame.Instance.GameStatus is CDGame.GameEnum.MapChange )
			return;

		var timer = TimeSpan.FromSeconds( CDGame.Instance.TimeRemaining );

		if ( CDGame.Instance.GameStatus == CDGame.GameEnum.Idle )
		{
			GameStats.ExtraText.SetText( $"Waiting for another player for {timer.ToString( @"m\:ss" )}" );
			Waves.SetClass( "hide", true );
			return;
		}

		if ( CDGame.Instance.GameStatus == CDGame.GameEnum.Starting )
		{
			GameStats.Timer.SetText( $"{timer.ToString( @"m\:ss" )}" );
			GameStats.ExtraText.SetText( "Starting" );
			Waves.SetClass( "hide", true );
			return;
		}

		if ( CDGame.Instance.GameStatus == CDGame.GameEnum.Post )
		{
			GameStats.Timer.SetText( $"{timer.ToString( @"m\:ss" )}" );
			GameStats.ExtraText.SetText( "Game Over" );
			Waves.SetClass( "hide", true );
			return;
		}

		switch ( CDGame.Instance.WaveStatus )
		{
			case CDGame.WaveEnum.Pre:
				GameStats.Timer.SetText( $"{timer.ToString( @"m\:ss")}" );
				GameStats.ExtraText.SetText( "Pre Wave" );
				Waves.SetClass( "hide", false);
				break;
			case CDGame.WaveEnum.Active:
				GameStats.Timer.SetText( $"{timer.ToString( @"m\:ss")}" );
				GameStats.ExtraText.SetText( "Active Wave" );
				Waves.SetClass( "hide", false);
				break;
			case CDGame.WaveEnum.Post:
				GameStats.Timer.SetText( $"{timer.ToString( @"m\:ss")}" );
				GameStats.ExtraText.SetText( "Post Wave" );
				Waves.SetClass( "hide", true);
				break;
		}
		GameStats.GameInfo.AddChild( Waves );

		var loopedString = "";

		if(CDGame.Instance.LoopedTimes > 1)
		{
			GameStats.GameInfo.AddChild( Loop );
			//gameInfo.GameInfo.AddChild( Loop );
			Loop.BigText.SetText( $"{CDGame.Instance.LoopedTimes - 1}" );
			Loop.SmallText.SetText( $"Loop" );
		}

		GameStats.IsCompetitive = CDGame.Instance.Competitive;

		Waves.SmallText.SetText( "Wave" );
		Waves.BigText.SetText( $"{CDGame.Instance.CurWave}/{CDGame.Instance.MaxWaves}{loopedString}" );

		SetClass( "debugOpen", DebugMenu.IsOpen );
	}
}
