using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public struct Rewards
{
	public int MinXP { get; set; }
	public int MaxXP { get; set; }
	public int MinCash { get; set; }
	public int MaxCash { get; set; }
}

public struct OverrideMaterial
{
	[ResourceType( "vmat" )]
	public Material Material { get; set; }
	public string Attribute { get; set; }
	//public Color Color { get; set; }
}

[GameResource( "CastleTD NPC", "npc", "Creates a new CastleTD NPC" )]
public class BaseNPCAsset : GameResource
{
	[Category( "Meta Info" )]
	public string Name { get; set; } = "Basic NPC";

	[Category( "Meta Info" )]
	public string Description { get; set; } = "Description Info";

	[Category("NPC Info"), MinMax(1, 1000)]
	public int StartHealth { get; set; } = 5;

	[Category( "NPC Info" ), MinMax( 0, 1000 ), ShowIf( "NPCType", SpecialType.Armoured )]
	public int StartArmor { get; set; } = 0;

	[Category( "NPC Info" ), MinMax( 1, 1000.0f )]
	public float Damage { get; set; } = 5.0f;

	[Category("NPC Info"), MinMax(0.1f, 100.0f)]
	public float Speed { get; set; } = 1.0f;

	[Category( "NPC Info" ), MinMax( 0.1f, 1.0f )]
	public float Scale { get; set; } = 0.5f;

	[Category( "NPC Info" ), Title("Is a Boss"), Description("Is this NPC a boss type")]
	public bool IsABoss { get; set; } = false;

	[Category("NPC Info")]
	public Rewards KillReward { get; set; }

	public enum SpecialType
	{
		Standard,
		Armoured,
		Hidden,
		Disruptor,
		Splitter,
		Airborne,
	}

	/// <summary>
	/// What kind of NPC is this?
	/// </summary>
	[Category( "NPC Info" ), Title("Type")]
	public SpecialType NPCType { get; set; } = SpecialType.Standard;

	[Category( "Splitter Info" ), ResourceType( "sound" ), ShowIf("NPCType", SpecialType.Splitter)]
	public string SplittingSound { get; set; } = "";

	/// <summary>
	/// Overrides the model, this should follow the citizen animgraph for animations
	/// </summary>
	[Category( "NPC Outfit + Design" ), ResourceType("vmdl")]
	public string OverrideModel { get; set; }

	/// <summary>
	/// Overrides the material with any attributes
	/// </summary>
	[Category( "NPC Outfit + Design" )]
	public OverrideMaterial[] MaterialOverrides { get; set; }

	[Category( "NPC Outfit + Design" )]
	public Color Color { get; set; } = Color.White;

	/// <summary>
	/// The hat clothing to apply on the NPC's head
	/// </summary>
	[Category( "NPC Outfit + Design" ), ResourceType( "vmdl" )]
	public string Hat { get; set; }

	/// <summary>
	/// The top clothing to apply on the NPC's chest
	/// </summary>
	[Category( "NPC Outfit + Design" ), ResourceType( "vmdl" )]
	public string Top { get; set; }

	/// <summary>
	/// The bottom clothing to apply on the NPC's leggings
	/// </summary>
	[Category( "NPC Outfit + Design" ), ResourceType( "vmdl" )]
	public string Bottom { get; set; }

	/// <summary>
	/// The foot clothing to apply on the NPC's feet
	/// </summary>
	[Category( "NPC Outfit + Design" ), ResourceType( "vmdl" )]
	public string Feet { get; set; }

	public static IReadOnlyList<BaseNPCAsset> ctd_npcs => _allNPCs;
	internal static List<BaseNPCAsset> _allNPCs = new();

	public IReadOnlyList<BaseNPCAsset> ReadAllNPCs()
	{
		return _allNPCs;
	}

	protected override void PostLoad()
	{
		base.PostLoad();

		if ( !_allNPCs.Contains( this ) )
			_allNPCs.Add( this );
	}
}

