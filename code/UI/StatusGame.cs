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
	public GameStats.GameInfoPanel gameInfo = new();
	public StatusGame()
	{
		StyleSheet.Load( "UI/StatusGame.scss" );

		AddChild( gameInfo );

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

		if( CDGame.Instance.GameStatus == CDGame.GameEnum.Starting )
		{
			gameInfo.WaveTimer.SetText( $"{timer.ToString( @"m\:ss" )}" );
			gameInfo.ExtraText.SetText( "Starting" );
			gameInfo.SetClass( "activeGame", true );
			return;
		}

		if(CDGame.Instance.GameStatus == CDGame.GameEnum.Post)
		{
			gameInfo.WaveTimer.SetText( $"{timer.ToString( @"m\:ss" )}" );
			gameInfo.ExtraText.SetText( "Game Over" );
			gameInfo.SetClass( "activeGame", false );
			return;
		}

		switch(CDGame.Instance.WaveStatus)
		{
			case CDGame.WaveEnum.Pre:
				gameInfo.WaveTimer.SetText( $"{timer.ToString( @"m\:ss" )}" );
				gameInfo.txtRoundPanel.SetClass( "hide", false );
				gameInfo.ExtraText.SetText( "Pre Wave" );
				break;
			case CDGame.WaveEnum.Active:
				gameInfo.WaveTimer.SetText( $"{timer.ToString( @"m\:ss" )}" );
				gameInfo.ExtraText.SetText( $"Active Wave" );
				gameInfo.txtRoundPanel.SetClass( "hide", false );
				gameInfo.SetClass( "activeGame", true );
				//gameInfo.ExtraText.SetText( $"Active Wave | Wave {CDGame.Instance.CurWave}/{CDGame.Instance.MaxWaves}" );
				//gameInfo.SetClass( "activeGame", false );
				break;
			case CDGame.WaveEnum.Post:
				gameInfo.WaveTimer.SetText( $"{ timer.ToString( @"m\:ss" )}" );
				gameInfo.ExtraText.SetText( "Post Wave" );
				gameInfo.SetClass( "activeGame", true );
				break;
		}

		gameInfo.RoundCounter.SetText( $"{CDGame.Instance.CurWave}/{CDGame.Instance.MaxWaves}" );
		//gameInfo.TextTimer = Math.Round(TimerElapsed).ToString();

		// WaveTimerSmall
		// You can show how much health the castle has when one of the terry's damages like
		// gameInfo.WaveTimerSmall.SetText("Health");
		// gameInfo.RoundCounter.SetText( $"{CDGame.Instance.CastleHealth}" );
	}
}
