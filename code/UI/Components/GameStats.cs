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
			public Label txtTimer;
			public Panel txtRoundPanel;
			public Label txtRound;
			public GameInfoPanel()
			{
				txtTimerPanel = Add.Panel("timer");
				txtTimer = txtTimerPanel.Add.Label( "00:00", "");
				txtRoundPanel = Add.Panel( "rounds" );
				txtRound = txtRoundPanel.Add.Label( "0/0", "" );

			}

			public string TextTimer
			{
				get { return txtTimer.Text; }
				set { txtTimer.Text = value; }
			}
			public string TextRounds
			{
				get { return txtRound.Text; }
				set { txtRound.Text = value; }
			}

		}
	}
}
