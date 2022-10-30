using System.Linq;
using Sandbox;

public sealed class TimeDisplacer : BaseSuperTower
{
	public override string TowerName => "Time Displacer";
	public override string TowerDesc => "A sort of time experiment able to teleport hostiles back to spawn";
	public override string TowerModel => "models/towers/timedisplacer.vmdl";
	public override int TowerCost => 650;
	public override float DeploymentTime => 6.5f; 
	public override string AttackSound => "timedisplacer_use";
	public override int RangeDistance => 64;

	public override void UseSuperAbility( TraceResult tr )
	{
		var ents = FindInSphere( tr.EndPosition, RangeDistance );

		var timeToRecover = 1.0f;

		foreach( var ent in ents )
		{
			if (ent is BaseNPC npc)
			{
				npc.Position = All.OfType<NPCSpawner>().FirstOrDefault().Position;
				npc.Steer.Target = All.OfType<NPCPath>().FirstOrDefault( x => x.StartNode ).Position;
				npc.TimeUntilSpecialRecover = timeToRecover;
				timeToRecover += 1.0f;
			}
		}

		base.UseSuperAbility(tr);
	}
}

