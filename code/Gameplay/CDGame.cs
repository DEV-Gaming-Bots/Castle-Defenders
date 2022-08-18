
using Sandbox;
using Sandbox.UI.Construct;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public partial class CDGame : Game
{
	public static GameStates CurrentState => (Current as CDGame)?.GameState ?? GameStates.Idle;

	public static CDGame Instance = Current as CDGame;

	[Net]
	public GameStates GameState { get; set; } = GameStates.Idle;

	public CDGame()
	{
		if(IsServer)
		{
			DebugMode = false;

			GameStatus = GameEnum.Idle;
			WaveStatus = WaveEnum.PreWave;
			DifficultyVariant = DiffVariants.None;
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

		var pawn = new CDPawn(client);
		pawn.Spawn();
		client.Pawn = pawn;
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
