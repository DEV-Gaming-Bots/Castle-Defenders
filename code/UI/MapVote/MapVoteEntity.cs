using Sandbox;
using System.Collections.Generic;
using System.Linq;
public partial class MapVoteEntity : Entity
{
	public static MapVoteEntity Current;
	MapSelectionMenu _panel;

	[Net]
	public IDictionary<IClient, string> Votes { get; set; }

	[Net]
	public string WinningMap { get; set; } = Game.Server.MapIdent.ToString();

	[Net]
	public RealTimeUntil VoteTimeLeft { get; set; } = 20;

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
		Current = this;
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		Current = this;
		_panel = new MapSelectionMenu();
		CDHUD.CurrentHud.AddChild( _panel );
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		_panel?.Delete();
		_panel = null;

		if ( Current == this )
			Current = null;
	}

	[Event.Client.Frame]
	public void OnFrame()
	{
		if ( _panel != null )
		{
			var seconds = VoteTimeLeft.Relative.FloorToInt().Clamp( 0, 60 );

			//_panel.TimeText = $"00:{seconds:00}";
		}
	}

	private void CullInvalidClients()
	{
		foreach ( var entry in Votes.Keys.Where( x => !x.IsValid() ).ToArray() )
		{
			Votes.Remove( entry );
		}
	}

	private void UpdateWinningMap()
	{
		if ( Votes.Count == 0 )
			return;

		WinningMap = Votes.GroupBy( x => x.Value ).OrderBy( x => x.Count() ).First().Key;
	}

	private void SetVote( IClient client, string map )
	{
		CullInvalidClients();
		Votes[client] = map;

		UpdateWinningMap();
		RefreshUI();
	}

	[ClientRpc]
	private void RefreshUI()
	{
		
	}

	[ConCmd.Server]
	public static void SetVote( string map )
	{
		if ( Current == null || ConsoleSystem.Caller == null )
			return;

		Current.SetVote( ConsoleSystem.Caller, map );
	}
}

