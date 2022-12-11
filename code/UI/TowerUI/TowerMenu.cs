using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Security.Cryptography.X509Certificates;

public  class TowerMenu : Panel
{
	public Panel TowerPnl;
	public Label TowerName;
	public Label TowerDesc;
	public Label TowerCost;
	public Label TowerOwnerAndPriority;
	public Label TowerStats;
	public Label Priority;
	public Label NextUpgrade;

	public Panel InputGlyphPnl;
	public Label PrimMouseLabel;
	public Image MousePrim;

	public Label SecondMouseLabel;
	public Image MouseSecond; 
	
	public Label UseLabel;
	public Image UseImage;

	public Panel mousePrimary;
	public Panel mouseSecondary;

	Entity curTowerHover;

	public TowerMenu()
	{
		StyleSheet.Load( "ui/towerui/towermenu.scss" );

		TowerPnl = Add.Panel("mainPnl");
		TowerName = TowerPnl.Add.Label( "", "towerName" );
		TowerDesc = TowerPnl.Add.Label( "", "towerDesc" );
		TowerCost = TowerPnl.Add.Label( "", "towerCost" );
		NextUpgrade = TowerPnl.Add.Label( "Next Upgrade: ???", "towerNextUpg" );
		TowerStats = TowerPnl.Add.Label( "Statistics: ???", "towerStats" );
		TowerOwnerAndPriority = TowerPnl.Add.Label( "", "towerOwner basetext" );
		Priority = TowerPnl.Add.Label( "", "towerPriority basetext" );

		InputGlyphPnl = TowerPnl.Add.Panel( "inputPnl" );
		Panel InputsPNL = InputGlyphPnl.Add.Panel( "inputsPnl" );

		mousePrimary = InputsPNL.Add.Panel( "mousePnl" );
		MousePrim = mousePrimary.Add.Image(null, "primMouse" );
		MousePrim.SetTexture( Input.GetGlyph( InputButton.PrimaryAttack ).ResourcePath );

		PrimMouseLabel = mousePrimary.Add.Label( "???", "text" );

		mouseSecondary = InputsPNL.Add.Panel( "mousePnl" );
		MouseSecond = mouseSecondary.Add.Image( null, "secondMouse" );
		MouseSecond.SetTexture( Input.GetGlyph( InputButton.SecondaryAttack ).ResourcePath );

		SecondMouseLabel = mouseSecondary.Add.Label( "???", "text" );

		//UseImage.SetTexture( Input.GetGlyph( InputButton.Use ).ResourcePath );
		UseLabel = InputGlyphPnl.Add.Label( "???", "text" );

	}

	[Event.Client.Frame]
	public void FrameScan()
	{
		var player = Game.LocalPawn as CDPawn;

		if ( player == null )
			return;

		var clTr = Trace.Ray( player.AimRay.Position, player.AimRay.Position + player.AimRay.Forward * 175 )
			.UseHitboxes( true )
			.WithTag( "tower" )
			.Run();

		DebugOverlay.Line( clTr.StartPosition, clTr.EndPosition );

		curTowerHover = clTr.Entity;
	}

	public override void Tick()
	{
		base.Tick();

		var player = Game.LocalPawn as CDPawn;

		if ( player == null )
			return;

		if(player.SelectedTower != null)
		{
			TowerName.SetText( player.SelectedTower.NetName );
			TowerDesc.SetText( player.SelectedTower.NetDesc );
			TowerCost.SetText( $"Build Cost: ${player.SelectedTower.NetCost}" );

			TowerOwnerAndPriority.SetText( "" );
			NextUpgrade.SetText( "" );
			TowerStats.SetText( "" );

			PrimMouseLabel.SetText( "Place Tower" );
			SecondMouseLabel.SetText( "" );
			UseLabel.SetText( "" );

			mousePrimary.Style.Set( "display: flex;" );
			mouseSecondary.Style.Set( "display: none;" );

			return;
		}

		if( curTowerHover is BaseSuperTower superTower)
		{
			TowerOwnerAndPriority.SetText( $"Owner: {superTower.Owner.Client.Name}" );
			TowerName.SetText( superTower.NetName );
			TowerDesc.SetText( superTower.NetDesc );
;
			PrimMouseLabel.SetText( "Use Active Ability, Press again to use" );
			SecondMouseLabel.SetText( "Cancel ability while using" );
			UseLabel.SetText( "" );

			TowerStats.SetText(superTower.NetAbility);
			return;
		}

		if ( curTowerHover is BaseTower tower )
		{

			TowerOwnerAndPriority.SetText( $"Owner: {tower.Owner.Client.Name}" );
			Priority.SetText( $"Priority: {tower.TargetPriority}" );

			TowerName.SetText( tower.NetName );
			TowerDesc.SetText( tower.NetDesc );

			if ( tower.NetCost != -1 )
			{
				TowerCost.SetText( $"Upgrade Cost: ${tower.NetCost}" );
				NextUpgrade.SetText( $"Next Upgrade: {tower.NetUpgradeDesc}" );
				mousePrimary.Style.Set( "display: flex;" );
			}
			else
			{
				TowerCost.SetText( "Max Level" );
				NextUpgrade.SetText( "" );
				mousePrimary.Style.Set( "display: none;" );
			}

			TowerStats.SetText( $"{tower.NetStats}" );
			mouseSecondary.Style.Set( "display: flex;" );

			PrimMouseLabel.SetText( "Upgrade" );
			SecondMouseLabel.SetText( "Sell" );
			UseLabel.SetText( "\nUSE KEY: Change Priority" );
		}

		TowerPnl.SetClass( "showMenu", curTowerHover is BaseTower || curTowerHover is BaseSuperTower );
		InputGlyphPnl.SetClass( "showInputs", curTowerHover is BaseTower || curTowerHover is BaseSuperTower );
	}
}
