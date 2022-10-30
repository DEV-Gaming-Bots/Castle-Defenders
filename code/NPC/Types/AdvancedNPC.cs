using Sandbox;

public sealed class Ice : BaseNPC
{
	public override string NPCName => "Ice Creature";
	public override float BaseHealth => 225;
	public override float BaseSpeed { get; set; } = 10;
	public override string BaseModel => "models/citizen/citizen.vmdl";
	public override int[] MinMaxCashReward => new[] { 20, 65 };
	public override int[] MinMaxEXPReward => new[] { 10, 70 };
	public override float NPCScale => 0.45f;
	public override float Damage => 30f;

	public override void Spawn()
	{
		base.Spawn();

		ApplyTextureClient( To.Everyone, "materials/npcs/ice.vmat" );
	}
}

public sealed class Magma : BaseNPC
{
	public override string NPCName => "Magma Creature";
	public override float BaseHealth => 275;
	public override float BaseSpeed { get; set; } = 17.5f;
	public override string BaseModel => "models/citizen/citizen.vmdl";
	public override int[] MinMaxCashReward => new[] { 15, 45 };
	public override int[] MinMaxEXPReward => new[] { 15, 60 };
	public override float NPCScale => 0.55f;
	public override float Damage => 25f;

	public override void Spawn()
	{
		base.Spawn();

		ApplyTextureClient( To.Everyone, "materials/npcs/magma.vmat" );
	}
}

public sealed class Void : BaseNPC
{
	public override string NPCName => "Void Creature";
	public override float BaseHealth => 450;
	public override float BaseSpeed { get; set; } = 12.5f;
	public override string BaseModel => "models/citizen/citizen.vmdl";
	public override int[] MinMaxCashReward => new[] { 50, 135 };
	public override int[] MinMaxEXPReward => new[] { 25, 105 };
	public override float NPCScale => 0.45f;
	public override float Damage => 40f;

	public override void Spawn()
	{
		base.Spawn();

		ApplyTextureClient( To.Everyone, "materials/npcs/void.vmat" );
	}
}
