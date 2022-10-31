using Sandbox;

public sealed class ZombieBoss : BaseNPC
{
	public override string NPCName => "Patient Zero";
	public override float BaseHealth => 150;
	public override float BaseSpeed { get; set; } = 9.75f;
	public override string BaseModel => "models/citizen/citizen.vmdl";
	public override int[] MinMaxCashReward => new[] { 10, 65 };
	public override int[] MinMaxEXPReward => new[] { 9, 35 };
	public override float NPCScale => 0.45f;
	public override float Damage => 25.0f;

	public override void Spawn()
	{
		base.Spawn();

		var coat = new ModelEntity( "models/citizen_clothes/jacket/labcoat.vmdl" );
		coat.SetParent( this, true );

		ApplyTexture( "materials/npcs/zombie.vmat" );
		ApplyTextureClient( To.Everyone, "materials/npcs/zombie.vmat" );
	}
}

public sealed class VoidBoss : BaseNPC
{
	public override string NPCName => "Void King";
	public override float BaseHealth => 1750;
	public override float BaseSpeed { get; set; } = 7.5f;
	public override string BaseModel => "models/citizen/citizen.vmdl";
	public override int[] MinMaxCashReward => new[] { 75, 175 };
	public override int[] MinMaxEXPReward => new[] { 45, 165 };
	public override float NPCScale => 0.85f;
	public override float Damage => 125.0f;

	public override void Spawn()
	{
		base.Spawn();

		ApplyTexture( "materials/npcs/void.vmat" );
		ApplyTextureClient( To.Everyone, "materials/npcs/void.vmat" );
	}
}


