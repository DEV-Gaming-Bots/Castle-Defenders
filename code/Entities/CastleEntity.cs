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
		castle.SetModel( "models/castle.vmdl" );
		castle.Position = Position;
		castle.Rotation = Rotation;
		castle.Spawn();
	}

	public void DamageCastle(float damage)
	{
		Health -= damage;

		if ( Health <= 0 )
			CDGame.Instance.EndGame( CDGame.WinningEnum.Lost );
	}


}
