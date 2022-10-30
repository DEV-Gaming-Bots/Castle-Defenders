using Sandbox;
using SandboxEditor;

[Library( "info_cd_castle" )]
[EditorModel( "models/castle.vmdl" )]
[Title( "Castle" ), Description( "Defines a point where the castle will spawn" )]
[HammerEntity]
public sealed partial class CastleEntity : ModelEntity
{
	public enum CastleTeam
	{
		Unknown,
		Blue,
		Red
	}

	[Net, Property( "CastleTeam" ), Description("Which team does this castle belong to")]
	public CastleTeam TeamCastle { get; set; } = CastleTeam.Unknown;

	[Net]
	public float CastleHealth { get; set; }

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

		var multiply = CDGame.Instance.Difficulty switch
		{
			CDGame.DiffEnum.Easy => 1,
			CDGame.DiffEnum.Medium => 2,
			CDGame.DiffEnum.Hard => 3,
			CDGame.DiffEnum.Extreme => 4
		};

		CastleHealth = 250.0f - 50.0f * (multiply - 1);
	}

	public void DamageCastle(float damage)
	{
		CastleHealth -= damage;

		if ( CastleHealth <= 0 && !CDGame.Instance.Competitive )
			CDGame.Instance.EndGame( CDGame.WinningEnum.Lost );
		else if ( CastleHealth <= 0 && CDGame.Instance.Competitive )
		{
			switch( TeamCastle )
			{
				case CastleTeam.Blue:
					CDGame.Instance.EndGame( CDGame.WinningEnum.RedWin );
					break;
				case CastleTeam.Red:
					CDGame.Instance.EndGame( CDGame.WinningEnum.BlueWin );
					break;
			}
		}
	}
}
