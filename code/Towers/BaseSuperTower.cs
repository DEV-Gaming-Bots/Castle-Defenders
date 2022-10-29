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

			switch( (Owner as CDPawn).CurTeam )
			{
				case CDPawn.TeamEnum.Blue:
					CDGame.Instance.ActiveSuperTowerBlue = true;
					break;

				case CDPawn.TeamEnum.Red:
					CDGame.Instance.ActiveSuperTowerRed = true;
					break;

				default:
					CDGame.Instance.ActiveSuperTowerBlue = true;
					break;
			}

			PlayDeployAnimRPC( To.Everyone );
		} 
		else
			NetCost = TowerCost;
		
		Tags.Add( "tower" );

		NetName = TowerName;
		NetDesc = TowerDesc;

	}

	[Event.Tick.Server]
	public override void SimulateTower()
	{
	}

	public virtual void UseSuperAbility(TraceResult tr)
	{
		FireEffectAtLocation(tr.EndPosition);

		Despawn();
	}

	public void Despawn()
	{
		switch ( (Owner as CDPawn).CurTeam )
		{
			case CDPawn.TeamEnum.Blue:
				CDGame.Instance.ActiveSuperTowerBlue = false;
				break;

			case CDPawn.TeamEnum.Red:
				CDGame.Instance.ActiveSuperTowerRed = false;
				break;

			default:
				CDGame.Instance.ActiveSuperTowerBlue = false;
				break;
		}
		
		Delete();
	}
}

