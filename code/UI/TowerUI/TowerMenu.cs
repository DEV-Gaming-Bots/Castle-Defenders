using System;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public partial class TowerMenu : Panel
{
	public Panel TowerPnl;
	public Label TowerName;
	public Label TowerDesc;
	public Label TowerCost;
	public Label TowerOwner;

	public TowerMenu()
	{
		StyleSheet.Load( "ui/towerui/towermenu.scss" );

		TowerPnl = Add.Panel("mainPnl");
		TowerName = TowerPnl.Add.Label( "", "towerName" );
		TowerDesc = TowerPnl.Add.Label( "", "towerDesc" );
		TowerCost = TowerPnl.Add.Label( "", "towerCost" );
		TowerOwner = TowerPnl.Add.Label( "", "towerOwner" );
	}

	public override void Tick()
	{
		base.Tick();

		var player = Local.Pawn;

		if ( player == null )
			return;

		var clTr = Trace.Ray( player.EyePosition, player.EyePosition + player.EyeRotation.Forward * 145 )
			.UseHitboxes( true )
			.WithTag( "tower" )
			.Run();

		if ( clTr.Entity is BaseTower tower )
		{
			TowerPnl.SetClass( "showMenu", true );

			TowerOwner.SetText( $"Owner: {tower.Owner.Client.Name}");

			TowerName.SetText( tower.NetName );
			TowerDesc.SetText( tower.NetDesc );

			if ( tower.IsPreviewing )
			{
				TowerCost.SetText( $"Build Cost: ${tower.NetCost}" );
			}
			else
			{
				if( tower.NetCost != -1)
					TowerCost.SetText( $"Upgrade Cost: ${tower.NetCost}" );
				else
					TowerCost.SetText( "Max Level" );;
			}
		}
		else
			TowerPnl.SetClass( "showMenu", false );
	}
}
