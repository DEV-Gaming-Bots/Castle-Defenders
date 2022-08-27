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
	public Label TowerStats;
	public Label NextUpgrade;

	public TowerMenu()
	{
		StyleSheet.Load( "ui/towerui/towermenu.scss" );

		TowerPnl = Add.Panel("mainPnl");
		TowerName = TowerPnl.Add.Label( "", "towerName" );
		TowerDesc = TowerPnl.Add.Label( "", "towerDesc" );
		TowerCost = TowerPnl.Add.Label( "", "towerCost" );
		NextUpgrade = TowerPnl.Add.Label( "Next Upgrade: ???", "towerNextUpg" );
		TowerStats = TowerPnl.Add.Label( "Statistics: ???", "towerStats" );
		TowerOwner = TowerPnl.Add.Label( "", "towerOwner" );
	}

	public override void Tick()
	{
		base.Tick();

		var player = Local.Pawn as CDPawn;

		if ( player == null )
			return;

		if(player.SelectedTower != null)
		{
			TowerName.SetText( player.SelectedTower.NetName );
			TowerDesc.SetText( player.SelectedTower.NetDesc );
			TowerCost.SetText( $"Build Cost: ${player.SelectedTower.NetCost}" );

			TowerOwner.SetText( "" );
			NextUpgrade.SetText( "" );
			TowerStats.SetText( "" );

			TowerPnl.SetClass( "showMenu", true );

			return;
		}

		var clTr = Trace.Ray( player.EyePosition, player.EyePosition + player.EyeRotation.Forward * 145 )
			.UseHitboxes( true )
			.WithTag( "tower" )
			.Run();

		if(clTr.Entity is BaseSuperTower superTower)
		{
			TowerPnl.SetClass( "showMenu", true );
			TowerOwner.SetText( $"Owner: {superTower.Owner.Client.Name}" );
			TowerName.SetText( superTower.NetName );
			TowerDesc.SetText( superTower.NetDesc );

			if( superTower.Owner == player)
			{
				TowerCost.SetText( "Use your 'Primary Fire' to use this ability" );
				NextUpgrade.SetText( "When using, use your 'Primary Fire' again on an area to activate" );
			}
			TowerStats.SetText("Ability: Reverts NPCs back to spawn, their health will not be restored");
			return;
		}

		if ( clTr.Entity is BaseTower tower )
		{
			TowerPnl.SetClass( "showMenu", true );

			TowerOwner.SetText( $"Owner: {tower.Owner.Client.Name}");

			TowerName.SetText( tower.NetName );
			TowerDesc.SetText( tower.NetDesc );

			if ( tower.NetCost != -1 )
			{
				TowerCost.SetText( $"Upgrade Cost: {tower.NetCost}" );
				NextUpgrade.SetText( $"Next Upgrade: {tower.NetUpgradeDesc}" );
			}
			else
			{
				TowerCost.SetText( "Max Level" );
				NextUpgrade.SetText( "" );
			}

			TowerStats.SetText( $"{tower.NetStats}" );


		}
		else
			TowerPnl.SetClass( "showMenu", false );
	}
}
