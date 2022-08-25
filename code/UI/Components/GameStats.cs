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
			[Obsolete]
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
					Panel txtRoundPanel = Add.Panel( "extraInfo" );
					SmallText = txtRoundPanel.Add.Label( "-", "SmallText" );
					BigText = txtRoundPanel.Add.Label( "-", "BigText" );
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

		public class GameStatsPanel : Panel
		{
			public Panel TimerHealth;

			public Panel txtRoundPanel;
			public Label Timer;

			public Panel HealthBarBlue;
			public Panel HealthBarRed;
			private Panel BlueHealth;
			private Panel RedHealth;


			public Panel GameInfo;
			public Label ExtraText;

			public GameStatsPanel()
			{
				BlueHealth = Add.Panel( "BlueHealthBar hide" );
				HealthBarBlue = BlueHealth.Add.Panel( "bar" );
				RedHealth = Add.Panel( "RedHealthBar hide" );
				HealthBarRed = RedHealth.Add.Panel( "bar" );
				Timer = Add.Label( "--:--", "timer" );
				///////////////
				TimerHealth = Add.Panel( "timerHealth" );
				TimerHealth.AddChild( BlueHealth );
				TimerHealth.AddChild( Timer );
				TimerHealth.AddChild( RedHealth );
				///////////////
				GameInfo = Add.Panel( "GameInfo" );
				///////////////
				ExtraText = Add.Label( "-", "extratxt" );
			}

			public bool IsComptetitive
			{
				set
				{
					Log.Info( value );
					switch( value )
					{
						case true:
							BlueHealth.SetClass( "hide", false );
							RedHealth.SetClass( "hide", false );
							break;
						case false:
							BlueHealth.SetClass( "hide", false );
							RedHealth.SetClass( "hide", true );
							break;
					}
				}
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
