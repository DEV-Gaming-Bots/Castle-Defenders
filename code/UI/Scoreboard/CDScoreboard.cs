using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

using System.Collections.Generic;
using System.Linq;
public partial class CDScoreboard<T> : Panel where T : CDScoreboardEntry, new()
{
	public Panel Canvas { get; protected set; }
	Dictionary<Client, T> Rows = new();

	public Panel Header { get; protected set; }

	public CDScoreboard()
	{
		StyleSheet.Load( "UI/Scoreboard/CDScoreboard.scss" );
		AddClass( "cdscoreboard" );

		AddHeader();

		Canvas = Add.Panel( "canvas" );
	}

	public override void Tick()
	{
		base.Tick();

		SetClass( "open", ShouldBeOpen() );

		if ( !IsVisible )
			return;

		//
		// Clients that were added
		//
		foreach ( var client in Client.All.Except( Rows.Keys ) )
		{
			var entry = AddClient( client );
			Rows[client] = entry;
		}

		foreach ( var client in Rows.Keys.Except( Client.All ) )
		{
			if ( Rows.TryGetValue( client, out var row ) )
			{
				row?.Delete();
				Rows.Remove( client );
			}
		}
	}

	public virtual bool ShouldBeOpen()
	{
		return Input.Down( InputButton.Score );
	}


	protected virtual void AddHeader()
	{
		Header = Add.Panel( "header" );
		Header.Add.Label( "Name", "name" );
		Header.Add.Label( "Kills", "kills" );
		Header.Add.Label( "Cash", "cash" );
		Header.Add.Label( "Level", "level" );
		Header.Add.Label( "Ping", "ping" );
	}

	protected virtual T AddClient( Client entry )
	{
		var p = Canvas.AddChild<T>();
		p.Client = entry;
		return p;
	}
}
