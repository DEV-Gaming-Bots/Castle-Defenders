using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace CastleDefenders.UI.Components
{
	public class GameStats : Panel
	{
		public class GameInfoPanel : Panel
		{
			public Panel txtTimerPanel;
			public Label WaveTimer;
			public Panel txtRoundPanel;
			public Label RoundCounter;
			public GameInfoPanel()
			{
				txtTimerPanel = Add.Panel("timer");
				WaveTimer = txtTimerPanel.Add.Label( "00:00", "");
				txtRoundPanel = Add.Panel( "rounds" );
				RoundCounter = txtRoundPanel.Add.Label( "0/0", "" );

			}

			public string TextTimer
			{
				get { return WaveTimer.Text; }
				set { WaveTimer.Text = value; }
			}
			public string TextRounds
			{
				get { return RoundCounter.Text; }
				set { RoundCounter.Text = value; }
			}
		}
	}
}
