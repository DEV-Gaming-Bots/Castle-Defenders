using Sandbox.UI;


public partial class CDHUD : RootPanel
{
	public static CDHUD CurrentHud;

	public CDHUD()
	{
		CurrentHud?.Delete();
		CurrentHud = null;

		StyleSheet.Load( "/Styles/Hud.scss" );

		AddChild<Chat>();
		AddChild<CDScoreboard<CDScoreboardEntry>>();

		AddChild<Status>();
		AddChild<VoiceList>();
		AddChild<StatusGame>();
		AddChild<TowerMenu>();
		AddChild<PlayerStatus>();
		AddChild<PlayerLoadout>();
		AddChild<VoiceSpeaker>();

		CurrentHud = this;


		//AddChild<DebugMenu>();

		//AddChild( new PlayerTeamSelect(
		//	() => { Log.Info( "BLUE SELECTED!" ); },
		//	() => { Log.Info( "RED SELECTED!" ); } ) 
		//);
	}
}
