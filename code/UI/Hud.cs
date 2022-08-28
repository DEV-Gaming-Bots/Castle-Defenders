using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;

public partial class CDHUD : RootPanel
{
	public static CDHUD CurrentHud;

	public Scoreboard<ScoreboardEntry> Scoreboard { get; set; }

	public CDHUD()
	{
		if( CurrentHud != null )
		{
			CurrentHud.Delete();
			CurrentHud = null;
		}


		AddChild<ChatBox>();
		Scoreboard = AddChild<Scoreboard<ScoreboardEntry>>();

		AddChild<Status>();
		AddChild<VoiceList>();
		AddChild<StatusGame>();
		AddChild<TowerMenu>();
		AddChild<PlayerStatus>();
		//AddChild<PlayerLoadout>();
		//AddChild( new PlayerTeamSelect(
		//	() => { Log.Info( "BLUE SELECTED!" ); },
		//	() => { Log.Info( "RED SELECTED!" ); } ) 
		//);

		CurrentHud = this;
	}
}
