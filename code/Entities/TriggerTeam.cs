using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using SandboxEditor;

[Library( "cd_comp_teamtrigger" )]
[Title( "Team Trigger" ), Description( "A team trigger, you should set 2 of these on each side" )]
[VisGroup( VisGroup.Logic )]
[HammerEntity]
public class TriggerTeam : BaseTrigger
{
	public enum TeamSideEnum
	{
		Unspecified,
		Blue,
		Red
	}

	[Property, Description("Which side is this team on, the opposite team will be affected by this")]
	public TeamSideEnum Team { get; set; }

	public override void Spawn()
	{
		ActivationTags.Add( "cdplayer" );
	}

	public override void OnTouchStart( Entity toucher )
	{
		if ( !CDGame.Instance.Competitive )
			return;

		if ( toucher is CDPawn player )
		{
			if (!Team.ToString().Contains(player.CurTeam.ToString()) )
			{
				player.OnOtherTeamSide = true;

				player.SelectedTower?.Delete();
				player.SelectedTower = null;

				player.PreviewTower?.Delete();
				player.PreviewTower = null;

				player.CurSuperTower = null;

			} 
			else
				player.OnOtherTeamSide = false;
			
			Log.Info( player.Client.Name );
		}
	}
}

