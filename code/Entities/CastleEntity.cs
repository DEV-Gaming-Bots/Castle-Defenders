using Sandbox;
using SandboxEditor;

[Library( "info_cd_castle" )]
[EditorModel( "models/castle.vmdl" )]
[Title( "Castle" ), Description( "Defines a point where the castle will spawn" )]
[HammerEntity]
public class CastleEntity : ModelEntity
{
	public enum CastleTeam
	{
		Unknown,
		Blue,
		Red
	}

	[Property( "CastleTeam" ), Description("Which team does this castle belong to")]
	public CastleTeam TeamCastle { get; set; } = CastleTeam.Unknown;

	public override void Spawn()
	{
		var castle = new ModelEntity();

		switch(TeamCastle)
		{
			case CastleTeam.Blue:
				castle.SetModel( "models/blue_castle.vmdl" );
				break;
			case CastleTeam.Red:
				castle.SetModel( "models/red_castle.vmdl" );
				break;
			default:
				castle.SetModel( "models/castle.vmdl" );
				break;
		}

		castle.Position = Position;
		castle.Rotation = Rotation;
		castle.Spawn();

		int multiply = 1;

		switch(CDGame.Instance.Difficulty)
		{
			case CDGame.DiffEnum.Easy:
				multiply = 1;
				break;
			case CDGame.DiffEnum.Medium:
				multiply = 2;
				break;
			case CDGame.DiffEnum.Hard:
				multiply = 3;
				break;
			case CDGame.DiffEnum.Extreme:
				multiply = 4;
				break;
		}

		Health = 250.0f - (50.0f * (multiply - 1));
	}

	public void DamageCastle(float damage)
	{
		Health -= damage;

		if ( Health <= 0 )
			CDGame.Instance.EndGame( CDGame.WinningEnum.Lost );
	}


}
