using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class CDScoreboardEntry : Panel
{
	public Client Client;

	public Label PlayerName;
	public Label Damage;
	public Label Kills;
	public Label Money;
	public Label Level;
	public Label Ping;

	public CDScoreboardEntry()
	{
		AddClass( "entry" );

		PlayerName = Add.Label( "PlayerName", "name" );
		Damage = Add.Label( "", "damage" );
		Kills = Add.Label( "", "kills" );
		Money = Add.Label( "", "Cash" );
		Level = Add.Label( "", "level" );
		Ping = Add.Label( "", "ping" );
	}

	private RealTimeSince _timeSinceUpdate = 0;

	public override void Tick()
	{
		base.Tick();

		if ( !IsVisible )
			return;

		if ( !Client.IsValid() )
			return;

		if ( _timeSinceUpdate < 0.1f )
			return;

		_timeSinceUpdate = 0;
		UpdateData();
	}

	public virtual void UpdateData()
	{
		PlayerName.Text = Client.Name;

		Damage.Text = (Client.Pawn as CDPawn).GetDamage().ToString();
		Kills.Text = Client.GetInt( "kills" ).ToString();
		Money.Text = (Client.Pawn as CDPawn).GetCash().ToString();
		Level.Text = (Client.Pawn as CDPawn).GetLevel().ToString();
		Ping.Text = Client.Ping.ToString();

		if(CDGame.Instance.Competitive)
		{
			var player = Client.Pawn as CDPawn;
			if ( player == null )
				return;

			SetClass( "comp_redteam", player.CurTeam == CDPawn.TeamEnum.Red);
			SetClass( "comp_blueteam", player.CurTeam == CDPawn.TeamEnum.Blue );
		}

		SetClass( "me", Client == Local.Client );
	}

	public virtual void UpdateFrom( Client client )
	{
		Client = client;
		UpdateData();
	}
}
