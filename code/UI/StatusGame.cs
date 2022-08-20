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

		if ( CDGame.Instance.GameStatus != CDGame.GameEnum.Active )
			return;

		Log.Info( CDGame.Instance.TimeRemaining );

		gameInfo.WaveTimer.SetText( Math.Round( CDGame.Instance.TimeRemaining, 2 ).ToString() );
		//gameInfo.TextTimer = Math.Round(TimerElapsed).ToString();
	}
}
