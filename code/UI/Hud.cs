using Sandbox;
using Sandbox.UI;

using Components.NotificationManager;


public partial class CDHUD : RootPanel
{
	public static CDHUD CurrentHud;
	public NotificationManagerUI notificationManager;
	public CDHUD()
	{
		CurrentHud?.Delete();
		CurrentHud = null;

		StyleSheet.Load( "/Styles/Hud.scss" );

		notificationManager = AddChild<NotificationManagerUI>();

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
		
		/*notificationManager.AddNotiffication( "Imported", NotificationType.Success );*/

		//AddChild<DebugMenu>();

		//AddChild( new PlayerTeamSelect(
		//	() => { Log.Info( "BLUE SELECTED!" ); },
		//	() => { Log.Info( "RED SELECTED!" ); } ) 
		//);
	}

/*	[ ConCmd.Client( "pushnotification" )]
	public void PushNotification( string message, NotificationType type = NotificationType.Info, float timer = 5f )
	{
		notificationManager.AddNotiffication( message, type, timer );
	}*/
}
