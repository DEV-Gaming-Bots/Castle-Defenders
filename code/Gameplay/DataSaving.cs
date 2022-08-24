using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Sandbox;

public interface IPlayerData
{
	int EXP { get; set; }
	int ReqEXP { get; set; }
	int Level { get; set; }
	string[] TowerSlots { get; set; }
}

public class PlayerData : IPlayerData
{
	public int EXP { get; set; }
	public int ReqEXP { get; set; }
	public int Level { get; set; }
	[Net] public string[] TowerSlots { get; set; }
}

public partial class CDGame
{
	public void SaveData( CDPawn player )
	{
		if ( player.Client.IsBot )
			return;

		Log.Info( "Commiting save for " + player.Client.Name );

		FileSystem.Data.WriteJson( player.Client.PlayerId + ".json", (IPlayerData)player );

		Log.Info( player.Client.Name + "'s data has been saved" );
	}

	public bool HasSavefile(Client cl)
	{
		return FileSystem.Data.ReadJson<PlayerData>( cl.PlayerId + ".json" ) != null;
	}

	public bool LoadSave( Client cl )
	{
		IPlayerData loadData = FileSystem.Data.ReadJson<PlayerData>( cl.PlayerId + ".json");

		if ( loadData is null )
			return false;

		(cl.Pawn as CDPawn).LoadStats( loadData );

		return true;
	}
}

