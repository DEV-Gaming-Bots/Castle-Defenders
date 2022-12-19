using System;
using System.Linq;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace CastleDefenders.UI.Components
{
	public  class GameStats : Panel
	{
		public  class GameInfoPanel : Panel
		{
			public Panel GameInfo;

			public Panel WaveTimerPanel;
			public Label WaveTimer;
			public Label WaveTimerSmall;

			public Panel ExtraTextPanel;
			public Label ExtraText;

			public Panel TxtRoundPanel;
			public Label RoundCounter;
			[Obsolete]
			public GameInfoPanel()
			{
				GameInfo = Add.Panel( "GameInfo" );
				WaveTimerPanel = GameInfo.Add.Panel("timer");
				WaveTimer = WaveTimerPanel.Add.Label( "-", "text");
				TxtRoundPanel = GameInfo.Add.Panel( "rounds hide" );
				WaveTimerSmall = TxtRoundPanel.Add.Label( "Wave", "waveText" );
				RoundCounter = TxtRoundPanel.Add.Label( "-", "text" );

				ExtraTextPanel = Add.Panel( "extraText" );
				ExtraText = ExtraTextPanel.Add.Label( "-" );
			}

			public class ExtraGameInfo : Panel
			{
				public Label SmallText;
				public Label BigText;
				public ExtraGameInfo()
				{
					var txtRoundPanel = Add.Panel( "extraInfo" );
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

		public  class GameStatsPanel : Panel
		{
			public Panel TimerHealth;

			public Panel TxtRoundPanel;
			public Label Timer;

			public Panel HealthBarBlue;
			public Panel HealthBarRed;
			private Panel _blueHealth;
			private Panel _redHealth;
			public Label BlueHealthText;
			public Label RedHealthText;

			public Panel GameInfo;
			public Label ExtraText;

			public bool IsCompetitive
			{
				set
				{
					switch ( value )
					{
						case true:
							_blueHealth.SetClass( "hide", false );
							_redHealth.SetClass( "hide", false );
							break;
						case false:
							_blueHealth.SetClass( "hide", false );
							_redHealth.SetClass( "hide", true );
							break;
					}
				}
			}

			public GameStatsPanel()
			{
				_blueHealth = Add.Panel( "BlueHealthBar hide" );
				HealthBarBlue = _blueHealth.Add.Panel( "bar" );
				BlueHealthText = HealthBarBlue.Add.Label("-/-");
				_redHealth = Add.Panel( "RedHealthBar hide" );
				HealthBarRed = _redHealth.Add.Panel( "bar" );
				RedHealthText = HealthBarRed.Add.Label("-/-");
				Timer = Add.Label( "--:--", "timer" );
				
				TimerHealth = Add.Panel( "timerHealth" );
				TimerHealth.AddChild( _blueHealth );
				TimerHealth.AddChild( Timer );
				TimerHealth.AddChild( _redHealth );
				
				GameInfo = Add.Panel( "GameInfo" );
				
				ExtraText = Add.Label( "-", "extratxt" );
			}

			public override void Tick()
			{
				base.Tick();
				
				if( !CDGame.Instance.Competitive )
				{
					var blueCastleHP = Entity.All.OfType<CastleEntity>().First( x => x.TeamCastle == CastleEntity.CastleTeam.Blue ).CastleHealth;
					BlueHealthText.SetText( $"❤️ {blueCastleHP}" );
					HealthBarBlue.Style.Width = Length.Percent( blueCastleHP );
				} 
				else if (CDGame.Instance.Competitive)
				{
					var redCastleHP = Entity.All.OfType<CastleEntity>().First( x => x.TeamCastle == CastleEntity.CastleTeam.Red ).CastleHealth;
					var blueCastleHP = Entity.All.OfType<CastleEntity>().First( x => x.TeamCastle == CastleEntity.CastleTeam.Blue ).CastleHealth;
					BlueHealthText.SetText( $"{blueCastleHP} ❤️" );
					RedHealthText.SetText( $"❤️ {redCastleHP}");

					HealthBarRed.Style.Width = Length.Pixels( redCastleHP * 2 );
					HealthBarBlue.Style.Width = Length.Pixels( blueCastleHP * 2 );
				}

				switch ( CDGame.Instance.Competitive )
				{
					case true:
						_blueHealth.SetClass( "hide", false );
						_redHealth.SetClass( "hide", false );
						break;
					case false:
						_blueHealth.SetClass( "hide", false );
						_redHealth.SetClass( "hide", true );
						break;
				}
			}
		}
	}
	public  class UserSelectUI
	{
		public  class GameTeamSelectPanel : Panel
		{
			private Panel _root;

			public GameTeamSelectPanel()
			{
				_root = Add.Panel( "" );
			}
		}

		public  class LoadoutSelectPanel : Panel
		{
			private Panel _root;
			private Label _title;
			public Panel Loadouts;
			public Panel SelectPanel;
			public Panel SelectedLoadoutPanel;

			public LoadoutSelectPanel()
			{
				_root = Add.Panel( "root" );
				_title = _root.Add.Label( "TOWER LOADOUTS", "title" );
				SelectPanel = _root.Add.Panel("select" );
				Loadouts = SelectPanel.Add.Panel( "loadouts" );
				SelectedLoadoutPanel = SelectPanel.Add.Panel( "loadouts" );
			}

			public  class Loadout : Panel
			{
				private int _lvlUnlock;
				public Panel loadout;
				
				public Loadout( string name, int levelUnlock, Action onClick )
				{
					_lvlUnlock = levelUnlock;
					loadout = Add.Panel( "loadout" );
					loadout.Add.Label( name );
				}

				public override void Tick()
				{
					base.Tick();

					if ( CDGame.Instance.GameStatus == CDGame.GameEnum.MapChange )
						return;

					var player = Game.LocalPawn as CDPawn;

					if ( player == null )
						return;

					//if ( player.GetLevel() >= lvlunlock )
					//{
					//	loadout.SetClass( "unlocked", true );
					//}
				}
			}

			public void AddLoudout( string name , int levelUnlock , Action onClick )
			{
				Loadouts.AddChild(new Loadout(name, levelUnlock, onClick));
			}
		}
	}
}
