using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class Priest : BaseNPC
{
	public override string NPCName => "Priest";
	public override float BaseHealth => 65;
	public override float BaseSpeed => 11.0f;
	public override string BaseModel => "models/citizen/citizen.vmdl";
	public override int[] MinMaxCashReward => new int[] { 1, 15 };
	public override int[] MinMaxEXPReward => new int[] { 1, 5 };
	public override float NPCScale => 0.55f;
	public override float Damage => 2.5f;

	public TimeUntil TimeToHeal;
	public float HealAmount = 2.5f;
	public float HealTime = 12.5f;
	public float HealRadius = 48.0f;

	public override void Spawn()
	{
		base.Spawn();

		TimeToHeal = HealTime;

		var hat = new ModelEntity( "models/citizen_clothes/hat/hat.tophat.vmdl" );
		hat.SetParent( this, true );
	}

	[Event.Tick.Server]
	public override void Tick()
	{
		base.Tick();

		if( TimeToHeal <= 0 )
		{
			var ents = FindInSphere(Position, HealRadius );

			foreach ( var ent in ents )
			{
				if (ent is BaseNPC npc && npc != this)
					npc.Health += HealAmount;
			}

			TimeToHeal = HealTime;
		}
	}
}
