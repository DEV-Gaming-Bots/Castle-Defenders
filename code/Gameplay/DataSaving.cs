using Sandbox;
using System.Collections;
using System.Collections.Generic;

public interface IPlayerData
{
	int EXP { get; set; }
	int ReqEXP { get; set; }
	int Level { get; set; }
	IDictionary<int, string> TowerSlots { get; set; }
}

public  class PlayerData : IPlayerData
{
	public int EXP { get; set; }
	public int ReqEXP { get; set; }
	public int Level { get; set; }
	public IDictionary<int, string> TowerSlots { get; set; }
}

public  partial class CDGame
{
	public void SaveData( CDPawn player )
	{
		if ( player.Client.IsBot )
			return;

		Log.Info( "Commiting save for " + player.Client.Name );

		FileSystem.Data.WriteJson( player.Client.SteamId + ".json", (IPlayerData)player );
		Log.Info( player.Client.Name + "'s data has been saved" );
	}

	public bool HasSavefile( IClient cl )
	{
		return FileSystem.Data.ReadJson<PlayerData>( cl.SteamId + ".json" ) != null;
	}

	public void LoadSave( IClient cl )
	{
		IPlayerData loadData = FileSystem.Data.ReadJson<PlayerData>( cl.SteamId + ".json");

		if ( loadData is null ) return;

		(cl.Pawn as CDPawn).LoadStats( loadData );
	}
}

