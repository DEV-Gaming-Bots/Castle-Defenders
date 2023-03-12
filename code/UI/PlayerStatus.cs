using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Linq;

public  class PlayerStatus : Panel
{
	public Panel PlayerPnl;
	public Label PlayerInfo;
	public Label PlayerCash;

	public PlayerStatus()
	{
		StyleSheet.Load( "ui/playerstatus.scss" );

		PlayerPnl = Add.Panel( "panel" );
		PlayerInfo = PlayerPnl.Add.Label( "???", "playerInfo" );
		PlayerCash = PlayerPnl.Add.Label( "???", "playerCash" );
	}

	public override void Tick()
	{
		base.Tick();

		if ( CDGame.Instance.GameStatus == CDGame.GameEnum.MapChange )
			return;

		var pawn = Game.LocalPawn;

		if ( pawn == null )
			return;

		var clTr = Trace.Ray( pawn.AimRay.Position, pawn.AimRay.Position + pawn.AimRay.Forward * 999 )
			.Ignore( pawn )
			.Run();

		if(clTr.Entity is CDPawn player)
		{
			PlayerInfo.SetClass( "gamejamwinner", player.IsSpecial );
			PlayerInfo.SetText( player.Client.Name );
			PlayerCash.SetText( $"Cash: ${player.GetCash()}" );
		}

		SetClass( "playerHover", clTr.Entity is CDPawn);
	}
}
