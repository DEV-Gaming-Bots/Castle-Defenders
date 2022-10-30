using System.Linq;
using Sandbox;
using SandboxEditor;

[Library( "cd_comp_setup" )]
[Title( "Competitive Setup" ), Description( "Sets up the map for competitive" )]
[HammerEntity]
public sealed class CompSetUp : Entity
{
	[Property, Description("The comp doors, will toggle depending if competitive is on or not")]
	public EntityTarget CompetitiveDoor { get; set; }

	public void SetUpCompGame()
	{
		var door = CompetitiveDoor.GetTargets().FirstOrDefault() as BrushEntity;

		if ( door == null )
			return;

		door.Enabled = false;
		door.Collisions = false;
	}
}
