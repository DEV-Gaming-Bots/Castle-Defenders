using Sandbox;
using Editor;

[Library( "info_cd_castle" )]
[EditorModel( "models/castle.vmdl" )]
[Title( "Castle" ), Description( "Defines a point where the castle will spawn" )]
[HammerEntity]
public  partial class CastleEntity : ModelEntity
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

	[Net]
	public float defaultCastleHealth { get; set; }

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

		switch ( CDGame.Instance.Difficulty )
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

		CastleHealth = 250.0f - 50.0f * multiply;
		defaultCastleHealth = CastleHealth;
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
