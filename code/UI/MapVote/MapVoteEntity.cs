using Sandbox;
using System.Collections.Generic;
using System.Linq;

sealed partial class MapVoteEntity : Entity
{
	static MapVoteEntity _current;
	MapVotePanel _panel;

	[Net]
	public IDictionary<Client, string> Votes { get; set; }

	[Net]
	public string WinningMap { get; set; } = "devbots.aperturelab";

	[Net]
	public RealTimeUntil VoteTimeLeft { get; set; } = 20;

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
		_current = this;
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		_current = this;
		_panel = new MapVotePanel();
		CDHUD.CurrentHud.AddChild( _panel );
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		_panel?.Delete();
		_panel = null;

		if ( _current == this )
			_current = null;
	}

	[Event.Frame]
	public void OnFrame()
	{
		if ( _panel != null )
		{
			var seconds = VoteTimeLeft.Relative.FloorToInt().Clamp( 0, 60 );

			_panel.TimeText = $"00:{seconds:00}";
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
		Log.Info( WinningMap );
	}

	private void SetVote( Client client, string map )
	{
		CullInvalidClients();
		Votes[client] = map;

		UpdateWinningMap();
		RefreshUI();
	}

	[ClientRpc]
	private void RefreshUI()
	{
		_panel.UpdateFromVotes( Votes );
	}

	[ConCmd.Server]
	public static void SetVote( string map )
	{
		if ( _current == null || ConsoleSystem.Caller == null )
			return;

		_current.SetVote( ConsoleSystem.Caller, map );
	}
}

