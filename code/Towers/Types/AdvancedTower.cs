using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

public partial class RadioactiveEmitter : BaseTower
{
	public override string TowerName => "Radioactive emitter Tower";
	public override string TowerDesc => "A tower that emits radiation to nearby hostiles";

	//Temporary until we get a pistol model
	public override string TowerModel => "models/towers/radioactivemitter.vmdl";
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
	public override int TowerCost => 350;
	public override float DeploymentTime => 3.67f;
	public override float AttackTime => 7.0f;
	public override float AttackDamage => 7.25f;
	public override int RangeDistance => 165;
	public override string AttackSound => "geiger_radiation";

	public override void Spawn()
	{
		base.Spawn();
	}

	[Event.Tick.Server]
	public override void SimulateTower()
	{
		if ( IsPreviewing )
			return;

		//Still deploying, wait until finished
		if ( TimeSinceDeployed < DeploymentTime )
			return;

		if ( TimeLastAttack < AttackTime )
			return;

		ScanForEnemies().ToList().ForEach( x => Attack( x ) );
	}

	[ClientRpc]
	public override void FireEffects()
	{
		base.FireEffects();
	}
}
