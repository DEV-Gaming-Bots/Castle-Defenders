using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class PlayerStatus : Panel
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

		var pawn = Local.Pawn;

		if ( pawn == null )
			return;

		var clTr = Trace.Ray( pawn.EyePosition, pawn.EyePosition + pawn.EyeRotation.Forward * 999 )
			.Ignore( pawn )
			.Run();

		if(clTr.Entity is CDPawn player)
		{
			PlayerInfo.SetText( player.Client.Name );
			PlayerCash.SetText( $"Cash: {player.GetCash()}" );
		}

		SetClass( "playerHover", clTr.Entity is CDPawn);

	}
}

