using Sandbox;
using Sandbox.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public sealed partial class Radar : BaseTower
{
	public override string TowerName => "Radar";
	public override string TowerDesc => "A scanning tower that detects hidden hostiles while also enhancing towers";

	public override string TowerModel => "models/towers/radartower.vmdl";
	public override int UnlockLevel => 0;
	public override BaseTower RequiredTowers => null;
	public override string[] TowerLevelDesc => new[]
	{
		"",
		"Upgraded radar dish to improve nearby towers",
		"More enhanced to scan further ranges to nearby towers",
		"Makes any nearby towers reach far distances"
	};

	public override List<(float AttTime, float AttDMG, int NewRange)> Upgrades => new()
	{
		new(-0.025f, 0, 0),
		new(-0.025f, 0, 0),
		new(-0.05f, 0, 0)
	};

	public override string[] TowerUpgradeDesc => new[]
	{
		$"Upgrades Nearby Speed {Upgrades[0].AttTime}",
		$"Upgrades Nearby Speed {Upgrades[1].AttTime}",
		$"Upgrades Nearby Speed {Upgrades[2].AttTime}",
		"",
	};

	public override int TowerMaxLevel => 4;
	public override int TowerCost => 325;
	public override int[] TowerLevelCosts => new[]
	{
		400,
		550,
		775,
		-1,
	};

	public override float DeploymentTime => 5.0f;
	public override float AttackTime { get; set; } = 0.0f;
	public override float AttackDamage { get; set; } = 0.0f;
	public override int RangeDistance { get; set; } = 100;
	public override string AttackSound => "";

	List<BaseTower> towersScanned;

	public override void Spawn()
	{
		towersScanned = new List<BaseTower>();
		base.Spawn();
	}

	[Event.Tick.Server]
	public override void SimulateTower()
	{
		if ( IsPreviewing )
			return;

		if ( TimeSinceDeployed < DeploymentTime )
			return;

		if ( !IsServer ) return;

		ValidateTowers();
		ScanForNearbyTowers();
		EnhanceTowers();
	}

	public override void SellTower()
	{
		if ( IsServer )
			RemoveEnhancement();

		base.SellTower();
	}

	public override void UpgradeTower()
	{
		if ( IsServer && CanUpgrade() ) 
			towersScanned.ToList().ForEach( x => x.HasEnhanced = false );

		base.UpgradeTower();
	}

	private void ValidateTowers()
	{
		foreach ( BaseTower tower in towersScanned.ToArray() )
		{
			if ( tower == null || !tower.IsValid )
				towersScanned.Remove( tower );
		}
	}

	private void RemoveEnhancement()
	{
		foreach ( BaseTower tower in towersScanned.ToArray())
		{
			if ( tower is Radar || tower == this )
				continue;

			tower.ResetAndRestoreStats();
			tower.CounterStealth = false;
			tower.HasEnhanced = false;
		}
	}

	private void EnhanceTowers()
	{
		foreach ( BaseTower tower in towersScanned.ToArray() )
		{
			if ( !tower.HasEnhanced )
			{
				tower.AttackTime += MathF.Round(AttackTime, 2);
				tower.RangeDistance += (int)MathF.Round( tower.RangeDistance / (4 + TowerLevel), 0);
				tower.CounterStealth = true;
				tower.HasEnhanced = true;

				tower.NetStats = $"Attack Delay {tower.AttackTime} | Damage {tower.AttackDamage} | Range {tower.RangeDistance}";
			}
		}
	}

	private void ScanForNearbyTowers()
	{
		var ents = FindInSphere( Position, RangeDistance );

		foreach ( var ent in ents )
		{
			if ( ent is BaseTower tower && !towersScanned.Contains( tower ) && !(tower is Radar || tower == this) && !tower.IsPreviewing )
				towersScanned.Add( tower );
		}
	}
}

