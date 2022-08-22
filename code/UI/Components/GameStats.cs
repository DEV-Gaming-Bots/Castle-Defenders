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
			public Label WaveTimerSmall;

			public Panel ExtraTextPanel;
			public Label ExtraText;

			public Panel txtRoundPanel;
			public Label RoundCounter;
			public GameInfoPanel()
			{
				GameInfo = Add.Panel( "GameInfo" );
				WaveTimerPanel = GameInfo.Add.Panel("timer");
				WaveTimer = WaveTimerPanel.Add.Label( "-", "text");
				txtRoundPanel = GameInfo.Add.Panel( "rounds hide" );
				WaveTimerSmall = txtRoundPanel.Add.Label( "Wave", "waveText" );
				RoundCounter = txtRoundPanel.Add.Label( "-", "text" );

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


	}
	public class UserSelectUI
	{
		public class GameTeamSelectPanel : Panel
		{
			private Panel root;

			public GameTeamSelectPanel()
			{
				root = Add.Panel( "" );
			}
		}

		public class LoadoutSelectPanel : Panel
		{
			private Panel root;
			private Label title;
			public Panel loadouts;

			public LoadoutSelectPanel()
			{
				root = Add.Panel( "root" );
				title = root.Add.Label( "TOWER LOADOUTS", "title" );
				loadouts = root.Add.Panel( "loadouts" );
				for ( int i = 0; i < 3; i++ )
				{
					Panel loadout = Add.Panel( "loadout" );
					loadout.Add.Label( "NAME HERE LMAO" );
					loadouts.AddChild( loadout );
				}
			}

			public void AddLoutout( Action OnClick )
			{

			}
		}
	}
}
