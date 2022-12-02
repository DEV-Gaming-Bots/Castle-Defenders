using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Security.Cryptography.X509Certificates;

public sealed class TowerMenu : Panel
{
	public Panel TowerPnl;
	public Label TowerName;
	public Label TowerDesc;
	public Label TowerCost;
	public Label TowerOwner;
	public Label TowerStats;
	public Label NextUpgrade;

	public Panel InputGlyphPnl;
	public Label PrimMouseLabel;
	public Image MousePrim;

	public Label SecondMouseLabel;
	public Image MouseSecond;

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


		InputGlyphPnl = Add.Panel( "inputPnl" );

		MousePrim = InputGlyphPnl.Add.Image(null, "primMouse" );
		MousePrim.SetTexture( Input.GetGlyph( InputButton.PrimaryAttack ).ResourcePath );

		PrimMouseLabel = InputGlyphPnl.Add.Label( "???", "text" );

		MouseSecond = InputGlyphPnl.Add.Image( null, "secondMouse" );
		MouseSecond.SetTexture( Input.GetGlyph( InputButton.SecondaryAttack ).ResourcePath );

		SecondMouseLabel = InputGlyphPnl.Add.Label( "???", "text" );

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
			InputGlyphPnl.SetClass( "showInputs", true );

			PrimMouseLabel.SetText( "Place Tower" );
			SecondMouseLabel.SetText( "" );
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

			InputGlyphPnl.SetClass( "showInputs", true );

			PrimMouseLabel.SetText( "Use Active Ability, Press again to use" );
			SecondMouseLabel.SetText( "Cancel ability while using" );

			TowerStats.SetText(superTower.NetAbility);
			return;
		}

		if ( clTr.Entity is BaseTower tower )
		{
			TowerPnl.SetClass( "showMenu", true );

			TowerOwner.SetText( $"Owner: {tower.Owner.Client.Name}" );

			TowerName.SetText( tower.NetName );
			TowerDesc.SetText( tower.NetDesc );

			if ( tower.NetCost != -1 )
			{
				TowerCost.SetText( $"Upgrade Cost: ${tower.NetCost}" );
				NextUpgrade.SetText( $"Next Upgrade: {tower.NetUpgradeDesc}" );
			}
			else
			{
				TowerCost.SetText( "Max Level" );
				NextUpgrade.SetText( "" );
			}

			TowerStats.SetText( $"{tower.NetStats}" );

			InputGlyphPnl.SetClass( "showInputs", true );

			PrimMouseLabel.SetText( "Upgrade" );
			SecondMouseLabel.SetText( "Sell" );
		}
		else
		{
			TowerPnl.SetClass( "showMenu", false );
			InputGlyphPnl.SetClass( "showInputs", false );
		}
	}
}
