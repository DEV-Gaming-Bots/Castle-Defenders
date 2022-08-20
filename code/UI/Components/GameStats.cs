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
			private Panel GameInfo;

			public Panel WaveTimerPanel;
			public Label WaveTimer;

			public Panel ExtraTextPanel;
			public Label ExtraText;

			public Panel txtRoundPanel;
			public Label RoundCounter;
			public GameInfoPanel()
			{
				GameInfo = Add.Panel( "GameInfo" );
				WaveTimerPanel = GameInfo.Add.Panel("timer");
				WaveTimer = WaveTimerPanel.Add.Label( "-", "");
				txtRoundPanel = GameInfo.Add.Panel( "rounds hide" );
				RoundCounter = txtRoundPanel.Add.Label( "-", "" );

				ExtraTextPanel = Add.Panel( "extraText" );
				ExtraText = ExtraTextPanel.Add.Label( "-" );
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
			public string TextExtra
			{
				get { return ExtraText.Text; }
				set { ExtraText.Text = value; }
			}

		}

		public class GameTeamSelect : Panel
		{
			private Panel root;

			public GameTeamSelect()
			{
				root = Add.Panel( "" );
			}
		}
	}
}
