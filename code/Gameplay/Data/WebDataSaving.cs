using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public partial class CDGame
{
	public WebSocket DataSocket;

	public async Task ConnectToDatabase()
	{
		DataSocket = new WebSocket();
		
		try
		{
			await DataSocket.Connect( DataURL );
		}
		catch ( Exception )
		{
			Log.Error( "Couldn't connect to the database, reverting to Host saving" );
		}
	}

	public void SaveToOnlineDatabase( CDPawn player )
	{
		if ( player.Client.IsBot || DataConfig != DataCFGEnum.Web )
			return;
	}

	public bool HasSavefileOnDatabase( IClient cl )
	{
		return false;
	}

	public void LoadSaveFromDatabase( IClient cl )
	{
		
	}
}

