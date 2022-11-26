using Sandbox;
using System.ComponentModel.DataAnnotations;
using SandboxEditor;
using System.Linq;

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
	public enum ModelType
	{
		Classic,
		Stone
	}

	[Property]
	public ModelType PortalType { get; set; } = ModelType.Classic;

	public override void Spawn()
	{
		var portal = new ModelEntity();
		
		if ( PortalType == ModelType.Classic )
		{
			portal.SetModel( "models/npc_portal.vmdl" );
		}

		if ( PortalType == ModelType.Stone )
		{
			portal.SetModel( "models/npc_portal_stone.vmdl" );
		}

		portal.Position = Position;
		portal.Rotation = Rotation;
		portal.Spawn();
	}

	public CastleEntity FindCastle()
	{
		return FindByName( CastleTarget ) as CastleEntity;
	}
}
