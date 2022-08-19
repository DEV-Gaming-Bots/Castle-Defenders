using System;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public partial class Status : Panel
{
	public Panel CashPnl;
	public Label CurCashLbl;

	public Status()
	{
		StyleSheet.Load( "UI/Status.scss" );

		CashPnl = Add.Panel();
		CurCashLbl = Add.Label("???", "text");
	}


	public override void Tick()
	{
		base.Tick();

		var player = Local.Pawn as CDPawn;

		if ( player == null )
			return;

		int plyCash = player.GetCash();

		CurCashLbl.SetText( plyCash.ToString("C0") );

	}
}
