
using Sandbox;
using Sandbox.UI.Construct;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public partial class CDGame : Game
{
	public static CDGame Instance => Current as CDGame;

	[ConVar.Replicated( "cd_competitve" )]
	public static bool StaticCompetitive { get; set; }

	[ConVar.Replicated( "cd_diff" )]
	public static DiffEnum StaticDifficulty { get; set; }

	public bool Competitive;

	public bool RefusePlay;

	public CDGame()
	{
		if(IsServer)
		{
			Debug = false;
			RefusePlay = false;
			DebugMode = DebugEnum.Default;

			GameStatus = GameEnum.Idle;
			WaveStatus = WaveEnum.Pre;
			DifficultyVariant = DiffVariants.None;

			Difficulty = StaticDifficulty;
			Competitive = StaticCompetitive;
		}

		if ( IsClient )
			_ = new CDHUD();
	}

	//TEMPORARY
	[Event.Hotload]
	public void UpdateHud()
	{
		if ( IsClient )
			_ = new CDHUD();
	}

	public override void DoPlayerSuicide( Client cl )
	{
		return;
	}

	public override void ClientJoined( Client client )
	{
		base.ClientJoined( client );

		var pawn = new CDPawn( client );
		pawn.Spawn();
		client.Pawn = pawn;

		if ( !HasSavefile( client ) )
			pawn.NewPlayerStats();
		else
			LoadSave( client );

		if ( GameStatus == GameEnum.Idle && !RefusePlay )
			StartGame();

		if ( GameStatus == GameEnum.Active )
			pawn.SetUpPlayer();
	}

	public override void PostLevelLoaded()
	{
		base.PostLevelLoaded();

		All.OfType<NPCPath>().ToList().ForEach( x => x.FindPaths() );
	}

	public override void ClientDisconnect( Client cl, NetworkDisconnectionReason reason )
	{
		if ( cl.Pawn is CDPawn ply )
		{
			SaveData( ply );

			foreach ( var tower in All.OfType<BaseTower>().ToList().Where(x => x.Owner == ply))
				tower.Delete();
		}

		base.ClientDisconnect( cl, reason );
	}

	public enum GameStates
	{
		Idle,
		TypeVote,
		PreWave,
		ActiveWave,
		Post,
		MapVote
	}
}
