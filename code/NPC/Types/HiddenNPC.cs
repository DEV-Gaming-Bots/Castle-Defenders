using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public partial class Spectre : BaseNPC
{
	public override string NPCName => "Spectral Entity";
	public override float BaseHealth => 50;
	public override float BaseSpeed { get; set; } = 10.0f;
	public override string BaseModel => "models/citizen/citizen.vmdl";
	public override int[] MinMaxCashReward => new int[] { 50, 135 };
	public override int[] MinMaxEXPReward => new int[] { 25, 105 };
	public override float NPCScale => 0.35f;
	public override float Damage => 15f;
	public override SpecialType NPCType => SpecialType.Hidden;

	public override void Spawn()
	{
		base.Spawn();

		RenderColor = Color.Gray.WithAlpha( 0.65f );
	}

	public override void SetUpPanel()
	{
		Panel = new NPCInfo( NPCName, Health, "Undetectable to most towers" );

	}
}
