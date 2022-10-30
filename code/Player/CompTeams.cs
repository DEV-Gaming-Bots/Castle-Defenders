using Sandbox;

public sealed partial class CDPawn
{
	public enum TeamEnum
	{
		Unknown,
		Blue,
		Red,
	}

	[Net]
	public TeamEnum CurTeam { get; set; }

	public bool OnOtherTeamSide;

	//Don't let opposing teams help each other
}
