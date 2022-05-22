using System;
using System.Linq;
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

public partial class TD2Game
{
	[Event("td2_evnt_savedata")]
	public void SaveData( TD2Pawn player )
	{
		if ( player.Client.IsBot )
			return;

		Log.Info( "Commiting save for " + player.Client.Name );

		FileSystem.Data.WriteJson( player.Client.PlayerId + ".json", (IPlayerData)player );

		Log.Info( player.Client.Name + "'s data has been saved" );
	}

	public bool LoadSave( TD2Pawn player )
	{
		if ( player.Client.IsBot )
			return false;

		PlayerData loadData = FileSystem.Data.ReadJson<PlayerData>( player.Client.PlayerId + ".json" );

		if ( loadData is null )
			return false;

		player.LoadStats( loadData );

		return true;
	}

	[ConCmd.Admin( "td2_forcesave" )]
	public static void SaveData()
	{
		var player = ConsoleSystem.Caller.Pawn as TD2Pawn;

		if ( player == null )
			return;

		Event.Run( "td2_evnt_savedata", player );
	}

	[ConCmd.Admin("td2_forcesave_all")]
	public static void SaveAllData()
	{
		foreach ( var player in Client.All.OfType<TD2Pawn>() )
		{
			if ( player == null )
				continue;

			Event.Run( "td2_evnt_savedata", player );
		}
	}
}

