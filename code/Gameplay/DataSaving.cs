using System;
using System.Linq;
using System.Numerics;
using Sandbox;

public interface IPlayerData
{
	int EXP { get; set; }
	int ReqEXP { get; set; }
	int Level { get; set; }
}

public class PlayerData : IPlayerData
{
	public int EXP { get; set; }
	public int ReqEXP { get; set; }
	public int Level { get; set; }
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

	public bool LoadSave( CDPawn player )
	{
		if ( player.Client.IsBot )
			return false;

		PlayerData loadData = FileSystem.Data.ReadJson<PlayerData>( player.Client.PlayerId + ".json" );

		if ( loadData is null )
			return false;

		player.LoadStats( loadData );

		return true;
	}
}

