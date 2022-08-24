using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public partial class CDPawn
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
