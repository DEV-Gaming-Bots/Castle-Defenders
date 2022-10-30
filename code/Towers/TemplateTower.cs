using System.Collections.Generic;
using Sandbox;

//THIS SHOULD ONLY BE USED TO CREATE NEW TOWERS
public sealed partial class Template : BaseTower
{
	public override string TowerName => "Template Tower";
	public override string TowerDesc => "A template tower";
	public override string TowerModel => "models/towers/pistol_temp.vmdl_c";
	public override int UnlockLevel => 0;
	public override BaseTower RequiredTowers => null;
	public override string[] TowerLevelDesc => new[]
	{
		"LEVEL 1 TEMPLATE DESCRIPTION",
		"LEVEL 2 TEMPLATE DESCRIPTION",
		"LEVEL 3 TEMPLATE DESCRIPTION",
		"LEVEL 4 TEMPLATE DESCRIPTION",
		"LEVEL 5 TEMPLATE DESCRIPTION"
	};
	public override List<(float AttTime, float AttDMG, int NewRange)> Upgrades => new();

	public override int[] TowerLevelCosts => new[]
	{
		1,
		2,
		3,
		4,
		-1,
	};
	public override int TowerMaxLevel => 5;
	public override int TowerCost => 1;
	public override float DeploymentTime => 0.5f;
	public override float AttackTime { get; set; } = 1.0f;
	public override float AttackDamage { get; set; } = 1.0f;
	public override int RangeDistance { get; set; } = 1;
	public override string AttackSound => "";

	[ClientRpc]
	public override void FireEffects()
	{
		base.FireEffects();
	}
}
