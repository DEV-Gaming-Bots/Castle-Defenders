using Sandbox.UI;

public  class CDHUD : RootPanel
{
	public static CDHUD CurrentHud;

	public CDHUD()
	{
		if( CurrentHud != null )
		{
			CurrentHud.Delete();
			CurrentHud = null;
		}


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

		CurrentHud = this;
	}
}
