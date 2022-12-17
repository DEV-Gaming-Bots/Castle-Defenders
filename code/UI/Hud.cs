using Sandbox;
using Sandbox.UI;

[Library]
public partial class CDHUD : RootPanel
{
	public static CDHUD CurrentHud;

	public CDHUD()
	{
		if ( !Game.IsClient )
			return;

		StyleSheet.Load( "/UI/Hud.scss" );

		AddChild<ChatBox>();
		AddChild<CDScoreboard<CDScoreboardEntry>>();

		AddChild<Status>();
		AddChild<VoiceList>();
		AddChild<StatusGame>();
		AddChild<TowerMenu>();
		AddChild<PlayerStatus>();
		AddChild<PlayerLoadout>();
		AddChild<VoiceSpeaker>();

		//AddChild<DebugMenu>();

		//AddChild( new PlayerTeamSelect(
		//	() => { Log.Info( "BLUE SELECTED!" ); },
		//	() => { Log.Info( "RED SELECTED!" ); } ) 
		//);
	}
}
