using System.Collections.Generic;
using System.Linq;
using Sandbox;

public sealed partial class Lightning : BaseTower
{
	public override string TowerName => "Lightning";
	public override string TowerDesc => "A tower that has the power of thunder, quite shocking";
	public override string TowerModel => "models/towers/lightningtower.vmdl";
	public override int UnlockLevel => 0;
	public override BaseTower RequiredTowers => null;
	public override int[] TowerLevelCosts => new[]
	{
		575,
		635,
		745,
		950,
		1350,
		-1
	};

	public override List<(float AttTime, float AttDMG, int NewRange)> Upgrades => new()
	{
		new(-0.10f, 5.0f, 0),
		new(-0.20f, 10.0f, 10),
		new(-0.35f, 12.5f, 15),
		new(-0.40f, 15.0f, 20),
		new(-0.45f, 17.5f, 25),
		new(-0.50f, 20.0f, 40),
	};

	public override string[] TowerLevelDesc => new[]
	{
		"",
		"An improved version allowing more energic attacks allowing for interlinked shock attacks",
		"An even more updated Lightning tower with better linked attacks, Harnessing more power from the storms",
		"Quite the shocking upgrade, allowing for more damage and links to other hostiles",
		"Even more shocking power, you don't wanna mess with this",
		"Surprisingly harnesses the power of Thor's thunder, who would have thought"
	};

	public override string[] TowerUpgradeDesc => new[]
	{
		$"Attack Speed +{-Upgrades[0].AttTime} | Damage +{Upgrades[0].AttDMG}",
		$"Attack Speed +{-Upgrades[1].AttTime} | Damage +{Upgrades[1].AttDMG} | Range +{Upgrades[1].NewRange}",
		$"Attack Speed +{-Upgrades[2].AttTime} | Damage +{Upgrades[2].AttDMG} | Range +{Upgrades[2].NewRange}",
		$"Attack Speed +{-Upgrades[3].AttTime} | Damage +{Upgrades[3].AttDMG} | Range +{Upgrades[3].NewRange}",
		$"Attack Speed +{-Upgrades[4].AttTime} | Damage +{Upgrades[4].AttDMG} | Range +{Upgrades[4].NewRange}",
		"",
	};

	public override int TowerMaxLevel => 6;
	public override int TowerCost => 525;
	public override float DeploymentTime => 2.25f;
	public override float AttackTime { get; set; } = 3.25f;
	public override float AttackDamage { get; set; } = 32.5f;
	public override int RangeDistance { get; set; } = 125;
	public override string AttackSound => "lightning_attack";

	private bool _charged;
	private int _shockNextLimit;
	private List<Particles> _shockParticles;

	public override void Spawn()
	{
		base.Spawn();
		CreateParticleList();
	}

	public override void UpgradeTower()
	{
		base.UpgradeTower();

		_shockNextLimit++;
	}

	[Event.Tick.Server]
	public override void SimulateTower()
	{
		base.SimulateTower();

		if ( Target == null )
		{
			TimeLastAttack = 0;
			_charged = false;
			return;
		}

		if ( TimeLastAttack * 2 >= AttackTime && !_charged )
		{
			ClearParticles( To.Everyone );
			PlaySound( "lightning_charge" );
			_charged = true;
		}
	}

	[ClientRpc]
	public void CreateParticleList()
	{
		_shockParticles = new List<Particles>();
	}

	[ClientRpc]
	public void ShockEffects(BaseNPC target, BaseNPC nextTarget = null)
	{
		Host.AssertClient();

		if ( _shockParticles == null )
			_shockParticles = new List<Particles>();

		if ( target == null )
			return;

		var lightning = Particles.Create( "particles/lightning_beam.vpcf" );

		if ( _shockParticles.Count <= 0 )
		{
			lightning.SetEntityAttachment( 1, this, "muzzle" );
			lightning.SetEntity( 0, target, Vector3.Up * 25 );
		} 
		else
		{
			if ( nextTarget == null )
				return;

			lightning.SetEntityAttachment( 1, target, "hat" );
			lightning.SetEntity( 0, nextTarget, Vector3.Up * 25 );
		}

		_shockParticles.Add( lightning );
	}

	[ClientRpc]
	public void ClearParticles()
	{
		if ( _shockParticles == null )
			return;

		_shockParticles.Clear();
	}

	protected override void OnDestroy()
	{
		ClearParticles( To.Everyone );
		base.OnDestroy();
	}

	public override void Attack( BaseNPC target )
	{
		base.Attack( target );
		_charged = false;

		ShockEffects( target );

		if ( _shockNextLimit <= 0 )
			return;

		var lastTargets = new List<BaseNPC>();
		lastTargets.Add( target );

		var shockTarget = target;

		for ( var i = 0; i < _shockNextLimit; i++ )
		{
			var ents = FindInSphere( shockTarget.Position, 48 );

			foreach ( var ent in ents )
			{
				if ( ent is BaseNPC npc && npc != shockTarget && !lastTargets.Contains( npc ) )
				{
					shockTarget = npc;
					break;
				}
			}

			if ( shockTarget == target )
				break;

			lastTargets.Add( shockTarget );

			ShockEffects( lastTargets[i], shockTarget );
			base.Attack( shockTarget );
		}
	}
}
