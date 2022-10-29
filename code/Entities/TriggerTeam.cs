using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using SandboxEditor;

[Library( "cd_comp_teamtrigger" )]
[Title( "Team Trigger" ), Description( "A team trigger, you should set 2 of these on each side" )]
[HammerEntity]
public partial class TriggerTeam : BaseTrigger
{
	
	[Property, Description("Which side is this team on, the opposite team will be affected by this")]
	public CDPawn.TeamEnum Team { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		ActivationTags.Clear();
		ActivationTags.Add( "cdplayer" );

	}

	public override void StartTouch( Entity other )
	{
		base.StartTouch( other );

		if ( !CDGame.Instance.Competitive )
			return;

		if ( other is CDPawn player )
		{
			if ( player.CurTeam != Team )
			{
				player.OnOtherTeamSide = true;

				player.DestroyPreview();

				player.SelectedTower?.Delete();
				player.SelectedTower = null;

				player.PreviewTower?.Delete();
				player.PreviewTower = null;

				player.CurSuperTower = null;

			} 
			else if (player.CurTeam == Team)
				player.OnOtherTeamSide = false;
		}
	}
}

