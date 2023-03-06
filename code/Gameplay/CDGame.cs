using Sandbox;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class CDGame : GameManager
{
	public static CDGame Instance => Current as CDGame;

	[ConVar.Replicated( "cd_competitive" )]
	public static bool StaticCompetitive { get; set; }

	[ConVar.Replicated( "cd_diff" )]
	public static DiffEnum StaticDifficulty { get; set; }

	[ConVar.Replicated( "cd_towerlimit" )]
	public static int StaticLimitTowers { get; set; }

	[Net] public bool Competitive { get; set; }

	[Net]
	public bool ActiveSuperTowerBlue { get; set; }

	[Net]
	public bool ActiveSuperTowerRed { get; set; }

	[Net]
	public IList<string> MapVoteIdents { get; set; }

	public int LimitTower;

	public CDGame()
	{
		if( Game.IsServer )
		{
			Debug = false;
			DebugMode = DebugEnum.Default;

			GameStatus = GameEnum.Idle;
			WaveStatus = WaveEnum.Pre;
			DifficultyVariant = DiffVariants.None;

			Difficulty = StaticDifficulty;
			Competitive = StaticCompetitive;

			ActiveSuperTowerBlue = false;
			ActiveSuperTowerRed = false;

			LoopedTimes = 1;

			LimitTower = StaticLimitTowers;
		}

		if ( Game.IsClient )
			_ = new CDHUD();
	}

	//TEMPORARY
	[Event.Hotload]
	public void UpdateHud()
	{
		if ( Game.IsClient )
		{
			_ = new CDHUD();

			ConsoleSystem.Run( "cd.update.slots" );
		}
	}

	public override async void PostLevelLoaded()
	{
		base.PostLevelLoaded();

		var mapList = await GetRemoteMapIdents();

		MapVoteIdents = mapList;
	}

	public override void ClientJoined( IClient client )
	{
		base.ClientJoined( client );

		var pawn = new CDPawn(client);
		client.Pawn = pawn;
		pawn.Spawn();
		pawn.SetTowerLimit( LimitTower );
		
		if ( !HasSavefile( client ) )
			pawn.NewPlayerStats();
		else
			LoadSave( client );

		if ( GameStatus == GameEnum.Idle )
		{
			if ( Competitive && CanPlayComp() )
			{
				StartCompGame();
				return;
			}

			if (!Competitive)
				StartGame();
		}

		if ( GameStatus == GameEnum.Active )
			pawn.SetUpPlayer();
	}

	public bool CanPlayComp()
	{
		if ( !All.OfType<CompSetUp>().Any() )
		{
			Log.Error( "This map does not have competitive support, switching to Co-Op" );
			Competitive = false;
			return false;
		}

		if ( Game.Clients.Count < 2 )
			return false;

		return true;
	}

	public override void ClientDisconnect( IClient cl, NetworkDisconnectionReason reason )
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
		foreach ( var client in Game.Clients )
		{
			if ( client.Pawn is CDPawn ply )
				SaveData( ply );
		}

		base.Shutdown();
	}

	private static async Task<List<string>> GetRemoteMapIdents()
	{
		var queryResult = await Package.FindAsync( $"type:map game:{Game.Server.GameIdent.Replace( "#local", "" )}", take: 99 );
		return queryResult.Packages.Select( ( p ) => p.FullIdent ).ToList();
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
