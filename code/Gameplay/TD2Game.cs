
using Sandbox;
using Sandbox.UI.Construct;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public partial class TD2Game : Game
{

	public static GameStates CurrentState => (Current as TD2Game)?.GameState ?? GameStates.Idle;

	[Net]
	public GameStates GameState { get; set; } = GameStates.Idle;

	TD2HUD oldHud;

	public TD2Game()
	{
		if(IsServer)
		{
			GameStatus = GameEnum.Idle;
			WaveStatus = WaveEnum.PreWave;
			DifficultyVariant = DiffVariants.None;
		}

		if ( IsClient )
			oldHud = new TD2HUD();
	}

	//TEMPORARY
	[Event.Hotload]
	public void UpdateHud()
	{
		oldHud?.Delete();

		if ( IsClient )
			oldHud = new TD2HUD();
	}

	public override void DoPlayerSuicide( Client cl )
	{
		return;
	}

	public override void ClientJoined( Client client )
	{
		base.ClientJoined( client );

		var pawn = new TD2Pawn(client);
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
