﻿using System;
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
				gameInfo.ExtraText.SetText( "Pre Wave" );
				break;
			case CDGame.WaveEnum.Active:
				gameInfo.WaveTimer.SetText( $"{timer.ToString( @"m\:ss" )}" );
				gameInfo.ExtraText.SetText( "Active Wave" );
				break;
			case CDGame.WaveEnum.Post:
				gameInfo.WaveTimer.SetText( $"{ timer.ToString( @"m\:ss" )}" );
				gameInfo.ExtraText.SetText( "Post Wave" );
				break;
		}

		gameInfo.RoundCounter.SetText( $"Wave {CDGame.Instance.CurWave}/{CDGame.Instance.MaxWaves}" );
		//gameInfo.TextTimer = Math.Round(TimerElapsed).ToString();
	}
}