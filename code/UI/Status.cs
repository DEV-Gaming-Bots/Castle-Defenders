using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class Status : Panel
{
	public Panel CashPnl;
	public Label CurCashLbl;
	public Panel EXPPnl;
	public Label CurEXPLbl;

	public Status()
	{
		StyleSheet.Load( "UI/Status.scss" );

		CashPnl = Add.Panel("cashPnl");
		CurCashLbl = CashPnl.Add.Label("???", "text");
		EXPPnl = Add.Panel( "expPnl" );
		CurEXPLbl = EXPPnl.Add.Label( "???", "text" );
	}


	public override void Tick()
	{
		base.Tick();

		if ( CDGame.Instance.GameStatus == CDGame.GameEnum.MapChange )
			return;

		var player = Game.LocalPawn as CDPawn;

		if ( player == null )
			return;

		var plyCash = player.GetCash();

		CurCashLbl.SetText( plyCash.ToString("C0") );
		CurEXPLbl.SetText( $"Level: {player.GetLevel()}\nXP: {player.GetEXP()}/{player.GetReqEXP()}" );

		SetClass( "debugOpen", DebugMenu.IsOpen );

	}
}
