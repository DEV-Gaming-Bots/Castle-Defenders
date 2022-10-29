using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
public partial class Husk : BaseNPC
{
	public override string NPCName => "Husk";
	public override float BaseHealth => 100;
	public override float BaseSpeed { get; set; } = 10;
	public override string BaseModel => "models/citizen/citizen.vmdl";
	public override int[] MinMaxCashReward => new int[] { 7, 33 };
	public override int[] MinMaxEXPReward => new int[] { 7, 30 };
	public override float NPCScale => 0.65f;
	public override SpecialType NPCType => SpecialType.Splitter;
	public override int SplitAmount => 3;
	public override float Damage => 10;

	public override void Spawn()
	{
		base.Spawn();
		RenderColor = new Color( 170, 170, 170 );

		ApplyTextureClient( To.Everyone, "materials/npcs/husk.vmat", "skin" );
	}

	public override void SetUpPanel()
	{
		Panel = new NPCInfo( NPCName, Health, "Splits into smaller versions on death" );
	}

	public void SpawnSplit()
	{
		var splitted = new SplitMinion();
		splitted.Spawn();
		
		splitted.ApplyTextureClient( To.Everyone, "materials/npcs/husk.vmat", "skin" );

		splitted.MinionName = NPCName + " Minion"; 

		splitted.RenderColor = RenderColor;
		splitted.CastleTarget = CastleTarget;
		splitted.Steer.Target = Steer.Target;

		splitted.Scale = NPCScale / 2;
		splitted.Health = BaseHealth / 2.5f;

		splitted.Position = Position + (Vector3.Random.x * 5) + (Vector3.Random.y * 5);
		splitted.Rotation = Rotation;

		splitted.BaseSpeed = BaseSpeed * 1.5f;
		splitted.CashReward = CashReward / 2;
		splitted.ExpReward = ExpReward / 2;
	}

	public override void OnKilled()
	{
		if ( LifeState == LifeState.Dead )
			return;

		base.OnKilled();

		for ( int i = 0; i < SplitAmount; i++ )
			SpawnSplit();
	}
}
