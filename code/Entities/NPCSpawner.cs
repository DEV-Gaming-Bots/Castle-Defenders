using Sandbox;
using SandboxEditor;

[Library( "info_cd_npcportal" )]
[EditorModel( "models/npc_portal.vmdl" )]
[Title( "NPC Spawn Gate" ), Description( "Defines a point where NPCs can spawn" )]
[HammerEntity]
public sealed class NPCSpawner : Entity
{
	[Property, FGDType("target_destination")]
	public string CastleTarget { get; set; }

	public CastleEntity CastleToAttack;
	
	public enum TeamEnum
	{
		Blue,
		Red
	}

	[Property]
	public TeamEnum AttackTeamSide { get; set; } = TeamEnum.Blue;

	public override void Spawn()
	{
		var portal = new ModelEntity();
		portal.SetModel( "models/npc_portal.vmdl" );
		portal.Position = Position;
		portal.Rotation = Rotation;
		portal.Spawn();
	}

	public CastleEntity FindCastle()
	{
		return FindByName( CastleTarget ) as CastleEntity;
	}
}
