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


	public TowerMenu()
	{
		StyleSheet.Load( "ui/towerui/towermenu.scss" );

		TowerPnl = Add.Panel("mainPnl");
		TowerName = TowerPnl.Add.Label( "", "towerName" );
		TowerDesc = TowerPnl.Add.Label( "", "towerDesc" );
		TowerCost = TowerPnl.Add.Label( "", "towerCost" );
	}

	public override void Tick()
	{
		base.Tick();

		var player = Local.Pawn;

		if ( player == null )
			return;

		var clTr = Trace.Ray( player.EyePosition, player.EyePosition + player.EyeRotation.Forward * 150 )
			.UseHitboxes( true )
			.WithTag( "tower" )
			.Run();

		if ( clTr.Entity is BaseTower tower )
		{
			TowerPnl.SetClass( "showMenu", true );

			TowerName.SetText( tower.NetName );
			TowerDesc.SetText( tower.NetDesc );
			TowerCost.SetText( $"${tower.NetCost}" );
		}
		else
			TowerPnl.SetClass( "showMenu", false );
	}
}
