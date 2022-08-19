using System;
using Sandbox;

public partial class Pistol : BaseTower
{
	public override string TowerName => "Pistol Tower";
	public override string TowerDesc => "A pistol tower";
	public override string TowerModel => "models/trickster_tower.vmdl_c";
	public override int UnlockLevel => 0;
	public override BaseTower RequiredTowers => null;
	public override string[] TowerLevelDesc => new string[]
	{
		"LEVEL 1 TEMPLATE DESCRIPTION",
		"LEVEL 2 TEMPLATE DESCRIPTION",
		"LEVEL 3 TEMPLATE DESCRIPTION",
		"LEVEL 4 TEMPLATE DESCRIPTION",
		"LEVEL 5 TEMPLATE DESCRIPTION"
	};

	public override int TowerMaxLevel => 5;
	public override int TowerCost => 1;
	public override float DeploymentTime => 0.35f;
	public override float AttackTime => 1.0f;
	public override int AttackDamage => 5;
	public override int RangeDistance => 175;
	public override string AttackSound => "pistol_fire";

	[ClientRpc]
	public override void FireEffects()
	{
		base.FireEffects();
	}
}
