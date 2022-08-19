using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
public partial class Husk : BaseNPC
{
	public override string NPCName => "Husk";
	public override int BaseHealth => 45;
	public override float BaseSpeed => 15;
	public override string BaseModel => "models/citizen/citizen.vmdl";
	public override int[] MinMaxCashReward => new int[] { 1, 10 };
	public override int[] MinMaxEXPReward => new int[] { 1, 5 };
	public override float NPCScale => 0.65f;
	public override SpecialType NPCType => SpecialType.Splitter;
	public override int SplitAmount => 3;
	public override float Damage => 10;

	public bool IsSplit;

	public override void Spawn()
	{
		base.Spawn();
		RenderColor = new Color( 170, 170, 170 );
	}

	public override void OnKilled()
	{
		base.OnKilled();

		if ( IsSplit )
			return;

		for ( int i = 0; i < SplitAmount; i++ )
		{
			var splitted = new Husk();
			splitted.IsSplit = true;
			splitted.Spawn();
			splitted.Scale = NPCScale / 2;
			splitted.PathTarget = PathTarget;
			splitted.Health /= 2;
			splitted.Position = Position + Vector3.Random * 35;
			splitted.Rotation = Rotation;
			splitted.BaseSpeed *= 1.5f;
		}
		
	}
}
