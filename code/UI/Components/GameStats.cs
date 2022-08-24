using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace CastleDefenders.UI.Components
{
	public class GameStats : Panel
	{
		public class GameInfoPanel : Panel
		{
			public Panel GameInfo;

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

			public class ExtraGameInfo : Panel
			{
				public Label SmallText;
				public Label BigText;
				public ExtraGameInfo()
				{
					Panel txtRoundPanel = Add.Panel( "rounds" );
					SmallText = txtRoundPanel.Add.Label( "-", "waveText" );
					BigText = txtRoundPanel.Add.Label( "-", "text" );
				}
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
			public Panel selectPanel;
			public Panel selectedLoadoutPanel;

			public LoadoutSelectPanel()
			{
				root = Add.Panel( "root" );
				title = root.Add.Label( "TOWER LOADOUTS", "title" );
				selectPanel = root.Add.Panel("select" );
				loadouts = selectPanel.Add.Panel( "loadouts" );
				selectedLoadoutPanel = selectPanel.Add.Panel( "loadouts" );
			}

			public class Loadout : Panel
			{
				private int lvlunlock;
				public Panel loadout;
				public Loadout( string Name, int LevelUnlock, Action OnClick )
				{
					lvlunlock = LevelUnlock;
					loadout = Add.Panel( "loadout" );
					loadout.Add.Label( Name );
				}

				public override void Tick()
				{
					base.Tick();

					if ( CDGame.Instance.GameStatus == CDGame.GameEnum.MapChange )
						return;

					var player = Local.Pawn as CDPawn;

					if ( player == null )
						return;

					//if ( player.GetLevel() >= lvlunlock )
					//{
					//	loadout.SetClass( "unlocked", true );
					//}
				}
			}

			public void AddLoutout( string Name , int LevelUnlock , Action OnClick )
			{
				loadouts.AddChild(new Loadout(Name, LevelUnlock, OnClick));
			}
		}
	}
}
