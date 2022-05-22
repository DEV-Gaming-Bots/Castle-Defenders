using System;
using Sandbox;

//THIS SHOULD ONLY BE USED TO CREATE NEW TOWERS
public partial class TemplateTower : BaseTower
{
	public override string TowerName => "Template Tower";
	public override string TowerDesc => "A template tower";
	public override string TowerModel => "models/towers/pistol_temp.vmdl_c";
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
	public override float DeploymentTime => 0.5f;
	public override float AttackTime => 1.0f;
	public override int AttackDamage => 5;
	public override int RangeDistance => 45;
	public override string AttackSound => "";

	[ClientRpc]
	public override void FireEffects()
	{
		base.FireEffects();
	}
}
