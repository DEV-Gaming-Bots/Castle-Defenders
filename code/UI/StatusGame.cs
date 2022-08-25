using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using CastleDefenders.UI.Components;

public class StatusGame : Panel
{
	public TimeSince TimerElapsed;
	//public GameStats.GameInfoPanel gameInfo = new();
	public GameStats.GameStatsPanel gameStats = new();
	public GameStats.GameInfoPanel.ExtraGameInfo Loop = new();
	public GameStats.GameInfoPanel.ExtraGameInfo Waves = new();
	public StatusGame()
	{
		StyleSheet.Load( "UI/StatusGame.scss" );

		//AddChild( gameInfo );
		AddChild( gameStats );

		//gameInfo.TextTimer = "bruh";
		//gameInfo.TextRounds = "lmao";
	}

	public override void Tick()
	{
		base.Tick();

		if ( CDGame.Instance.GameStatus == CDGame.GameEnum.MapChange )
			return;

		if ( CDGame.Instance.GameStatus == CDGame.GameEnum.Idle )
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
			gameStats.Timer.SetText( $"{timer.ToString( @"m\:ss" )}" );
			gameStats.ExtraText.SetText( "Starting" );
			Waves.SetClass( "hide", true );
			return;
		}

		if ( CDGame.Instance.GameStatus == CDGame.GameEnum.Post )
		{
			gameStats.Timer.SetText( $"{timer.ToString( @"m\:ss" )}" );
			gameStats.ExtraText.SetText( "Game Over" );
			Waves.SetClass( "hide", true );
			return;
		}

		switch ( CDGame.Instance.WaveStatus )
		{
			case CDGame.WaveEnum.Pre:
				gameStats.Timer.SetText( $"{timer.ToString( @"m\:ss")}" );
				gameStats.ExtraText.SetText( "Pre Wave" );
				Waves.SetClass( "hide", false);
				break;
			case CDGame.WaveEnum.Active:
				gameStats.Timer.SetText( $"{timer.ToString( @"m\:ss")}" );
				gameStats.ExtraText.SetText( "Active Wave" );
				Waves.SetClass( "hide", false);
				break;
			case CDGame.WaveEnum.Post:
				gameStats.Timer.SetText( $"{timer.ToString( @"m\:ss")}" );
				gameStats.ExtraText.SetText( "Post Wave" );
				Waves.SetClass( "hide", true);
				break;
		}
		gameStats.GameInfo.AddChild( Waves );

		string loopedString = "";

		if(CDGame.Instance.LoopGame && CDGame.Instance.LoopedTimes > 1)
		{
			gameStats.GameInfo.AddChild( Loop );
			//gameInfo.GameInfo.AddChild( Loop );
			Loop.BigText.SetText( $"{CDGame.Instance.LoopedTimes - 1}" );
			Loop.SmallText.SetText( $"Loop" );
		}

		gameStats.IsCompetitive = CDGame.StaticCompetitive;

		Waves.SmallText.SetText( "Wave" );
		Waves.BigText.SetText( $"{CDGame.Instance.CurWave}/{CDGame.Instance.MaxWaves}{loopedString}" );
	}
}
