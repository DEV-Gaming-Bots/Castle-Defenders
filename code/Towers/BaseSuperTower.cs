using Sandbox;
using System.Net;

public partial class BaseSuperTower : BaseTower
{
	public override string TowerName => "BASE SUPER TOWER";
	public override string TowerDesc => "BASE SUPER TOWER DESC";
	public override string TowerModel => "";
	public override int TowerCost => 1;
	public override int UnlockLevel => 0;
	public override int RangeDistance => 25;

	public override void Spawn()
	{
		SetModel( TowerModel );
		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );

		if ( !IsPreviewing )
		{
			TimeSinceDeployed = 0;
			NetCost = -1;

			CDGame.Instance.ActiveSuperTower = true;

			PlayDeployAnimation();
		} 
		else
			NetCost = TowerCost;
		
		Tags.Add( "tower" );

		NetName = TowerName;
		NetDesc = TowerDesc;

	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();
	}

	[Event.Tick.Server]
	public override void SimulateTower()
	{
	}

	public virtual void UseSuperAbility(TraceResult tr)
	{
		Sound.FromWorld( AttackSound, tr.EndPosition );
		Despawn();
	}

	public void Despawn()
	{
		CDGame.Instance.ActiveSuperTower = false;
		Delete();
	}
}

