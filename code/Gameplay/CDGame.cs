
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

public partial class CDGame : Game
{
	public static CDGame Instance => Current as CDGame;

	[ConVar.Replicated( "cd_competitve" )]
	public static bool StaticCompetitive { get; set; }

	[ConVar.Replicated( "cd_diff" )]
	public static DiffEnum StaticDifficulty { get; set; }

	[ConVar.Replicated( "cd_loopgame" )]
	public static bool StaticLoopGame { get; set; }

	[ConVar.Replicated( "cd_towerlimit" )]
	public static int StaticLimitTowers { get; set; }

	[Net] public bool Competitive { get; set; }

	public bool RefusePlay;

	[Net]
	public bool ActiveSuperTowerBlue { get; set; }

	[Net]
	public bool ActiveSuperTowerRed { get; set; }

	public int LimitTower;

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
			LoopGame = StaticLoopGame;

			ActiveSuperTowerBlue = false;
			ActiveSuperTowerRed = false;

			LoopedTimes = 1;

			LimitTower = StaticLimitTowers;
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
		pawn.SetTowerLimit( LimitTower );
		client.Pawn = pawn;

		if ( !HasSavefile( client ) )
			pawn.NewPlayerStats();
		else
			LoadSave( client );

		if ( GameStatus == GameEnum.Idle && !RefusePlay )
		{
			if ( Competitive && CanPlayComp() )
			{
				StartCompGame();
				return;
			} 
			else if (!Competitive)
				StartGame();
		}
		if ( GameStatus == GameEnum.Active )
			pawn.SetUpPlayer();
	}

	public bool CanPlayComp()
	{
		if ( All.OfType<CompSetUp>().Count() <= 0 )
		{
			Log.Error( "This map does not have competitive support, switching to Co-Op" );
			Competitive = false;
			return false;
		}

		if ( Client.All.Count() < 2 )
			return false;

		return true;
	}

	public override void PostLevelLoaded()
	{
		base.PostLevelLoaded();
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

	public override void Shutdown()
	{
		foreach ( Client client in Client.All )
		{
			if ( client.Pawn is CDPawn ply )
				SaveData( ply );
		}

		base.Shutdown();
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
