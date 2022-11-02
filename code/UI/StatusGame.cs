using System;
using Sandbox;
using Sandbox.UI;
using CastleDefenders.UI.Components;

public sealed class StatusGame : Panel
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

		if ( CDGame.Instance.GameStatus is CDGame.GameEnum.MapChange or CDGame.GameEnum.Idle )
			return;

		var timer = TimeSpan.FromSeconds( CDGame.Instance.TimeRemaining );

		// REMOVE COMMENTED LATER!

		//if( CDGame.Instance.GameStatus == CDGame.GameEnum.Starting )
		//{
		//	gameInfo.WaveTimer.SetText( $"{timer.ToString( @"m\:ss" )}" );
		//	gameInfo.ExtraText.SetText( "Starting" );
		//	gameInfo.SetClass( "activeGame", true );
		//	gameInfo.txtRoundPanel.SetClass( "hide", true );
		//	return;
		//}
		//
		//if(CDGame.Instance.GameStatus == CDGame.GameEnum.Post)
		//{
		//	gameInfo.WaveTimer.SetText( $"{timer.ToString( @"m\:ss" )}" );
		//	gameInfo.ExtraText.SetText( "Game Over" );
		//	gameInfo.SetClass( "activeGame", false );
		//	return;
		//}

		//switch(CDGame.Instance.WaveStatus)
		//{
		//	case CDGame.WaveEnum.Pre:
		//		gameInfo.WaveTimer.SetText( $"{timer.ToString( @"m\:ss" )}" );
		//		gameInfo.txtRoundPanel.SetClass( "hide", false );
		//		gameInfo.ExtraText.SetText( "Pre Wave" );
		//		break;
		//	case CDGame.WaveEnum.Active:
		//		gameInfo.WaveTimer.SetText( $"{timer.ToString( @"m\:ss" )}" );
		//		gameInfo.ExtraText.SetText( $"Active Wave" );
		//		gameInfo.txtRoundPanel.SetClass( "hide", false );
		//		gameInfo.SetClass( "activeGame", true );
		//		break;
		//	case CDGame.WaveEnum.Post:
		//		gameInfo.WaveTimer.SetText( $"{ timer.ToString( @"m\:ss" )}" );
		//		gameInfo.ExtraText.SetText( "Post Wave" );
		//		gameInfo.SetClass( "activeGame", true );
		//		break;
		//}

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

		if(CDGame.Instance.LoopGame && CDGame.Instance.LoopedTimes > 1)
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
