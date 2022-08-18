using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public partial class CDGame
{
	public bool DebugMode;

	[ConCmd.Admin("cd_debug")]
	public static void DebugToggle()
	{
		Instance.DebugMode = !Instance.DebugMode;

		Log.Info( "Debug Mode: " + Instance.DebugMode );
	}

	[ConCmd.Admin( "cd_npc_create" )]
	public static void SpawnNPC(string npcName)
	{
		if(!Instance.DebugMode )
		{
			Log.Error( "Debug is turned off" );
			return;
		}

		var npc = TypeLibrary.Create<BaseNPC>( npcName );

		if( npc == null )
		{
			Log.Error( "Invalid npc name" );
			return;
		}

		npc.Spawn();
	}
}
