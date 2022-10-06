using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public partial class Ice : BaseNPC
{
	public override string NPCName => "Ice Creature";
	public override float BaseHealth => 225;
	public override float BaseSpeed { get; set; } = 10;
	public override string BaseModel => "models/citizen/citizen.vmdl";
	public override int[] MinMaxCashReward => new int[] { 20, 65 };
	public override int[] MinMaxEXPReward => new int[] { 10, 70 };
	public override float NPCScale => 0.45f;
	public override float Damage => 30f;

	public override void Spawn()
	{
		base.Spawn();

		ApplyTextureClient( To.Everyone, "materials/npcs/ice.vmat" );
	}
}

public partial class Magma : BaseNPC
{
	public override string NPCName => "Magma Creature";
	public override float BaseHealth => 275;
	public override float BaseSpeed { get; set; } = 17.5f;
	public override string BaseModel => "models/citizen/citizen.vmdl";
	public override int[] MinMaxCashReward => new int[] { 15, 45 };
	public override int[] MinMaxEXPReward => new int[] { 15, 60 };
	public override float NPCScale => 0.55f;
	public override float Damage => 25f;

	public override void Spawn()
	{
		base.Spawn();

		ApplyTextureClient( To.Everyone, "materials/npcs/magma.vmat" );
	}
}

public partial class Void : BaseNPC
{
	public override string NPCName => "Void Creature";
	public override float BaseHealth => 450;
	public override float BaseSpeed { get; set; } = 12.5f;
	public override string BaseModel => "models/citizen/citizen.vmdl";
	public override int[] MinMaxCashReward => new int[] { 50, 135 };
	public override int[] MinMaxEXPReward => new int[] { 25, 105 };
	public override float NPCScale => 0.45f;
	public override float Damage => 40f;

	public override void Spawn()
	{
		base.Spawn();

		ApplyTextureClient( To.Everyone, "materials/npcs/void.vmat" );
	}
}
