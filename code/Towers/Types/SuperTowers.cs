using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public partial class TimeDisplacer : BaseSuperTower
{
	public override string TowerName => "Time Displacer";
	public override string TowerDesc => "A sort of time experiment able to teleport hostiles back to spawn";
	public override string TowerModel => "models/towers/trickstertower.vmdl";
	public override int TowerCost => 650;
	public override float DeploymentTime => 5.0f; 
	public override string AttackSound => "timedisplacer_use";
	public override int RangeDistance => 64;

	public override void UseSuperAbility( TraceResult tr )
	{
		var ents = FindInSphere( tr.EndPosition, RangeDistance );

		float timeToRecover = 1.0f;

		foreach( var ent in ents )
		{
			if (ent is BaseNPC npc)
			{
				npc.Position = All.OfType<NPCSpawner>().FirstOrDefault().Position;
				npc.Steer.Target = All.OfType<NPCPath>().Where( x => x.StartNode ).FirstOrDefault().Position;
				npc.TimeUntilSpecialRecover = timeToRecover;
				timeToRecover += 1.0f;
			}
		}

		base.UseSuperAbility(tr);
	}
}

