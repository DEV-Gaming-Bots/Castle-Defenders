using Sandbox;

[Library( "info_td2_castle" )]
[EditorModel( "models/towers/castle.vmdl" )]
[Description( "Defines a point where the castle will spawn" )]

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
		castle.SetModel( "models/towers/castle.vmdl" );
		castle.Position = Position;
		castle.Rotation = Rotation;
		castle.Spawn();
	}
}
