using System;
using Sandbox;


public partial class Lightning : BaseTower
{
	public override string TowerName => "Lightning Tower";
	public override string TowerDesc => "A tower that has the power of thunder, quite shocking";
	public override string TowerModel => "models/towers/lightningtower.vmdl";
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
	public override int TowerCost => 225;
	public override float DeploymentTime => 2.25f;
	public override float AttackTime => 3.25f;
	public override float AttackDamage => 32.5f;
	public override int RangeDistance => 125;
	public override string AttackSound => "lightning_attack";

	bool charged = false;

	[Event.Tick.Server]
	public override void SimulateTower()
	{
		base.SimulateTower();

		if ( Target == null )
		{
			TimeLastAttack = 0;
			charged = false;
			return;
		}

		if ( (TimeLastAttack * 2) >= AttackTime && !charged )
		{
			PlaySound( "lightning_charge" );
			charged = true;
		}
	}

	public override void Attack( BaseNPC target )
	{
		base.Attack( target );
		charged = false;
	}

	[ClientRpc]
	public override void FireEffects()
	{
		if ( IsPreviewing )
			return;

		base.FireEffects();
	}
}
