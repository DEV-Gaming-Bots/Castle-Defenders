using Sandbox;
using System;
using System.Linq;
using System.Threading.Tasks;

public partial class Peasant : BaseNPC
{
	public override string NPCName => "Peasant";
	public override float BaseHealth => 15;
	public override float BaseSpeed => 15;
	public override string BaseModel => "models/citizen/citizen.vmdl";
	public override int[] MinMaxCashReward => new int[] { 1, 15 };
	public override int[] MinMaxEXPReward => new int[] { 1, 5 };
	public override float NPCScale => 0.45f;
	public override float Damage => 2.5f;

	public override void Spawn()
	{
		base.Spawn();
	}
}

public partial class Zombie : BaseNPC
{
	public override string NPCName => "Zombie";
	public override float BaseHealth => 65;
	public override float BaseSpeed => 12.5f;
	public override string BaseModel => "models/citizen/citizen.vmdl";
	public override int[] MinMaxCashReward => new int[] { 6, 24 };
	public override int[] MinMaxEXPReward => new int[] { 4, 13 };
	public override float NPCScale => 0.45f;
	public override float Damage => 5.0f;

	bool hasLoaded = false;

	public override void Spawn()
	{
		base.Spawn();

		ApplyTextureClient( To.Everyone, "materials/npcs/zombie.vmat" );
	}
}
