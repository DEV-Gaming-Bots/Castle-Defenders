using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public partial class Riot : BaseNPC
{
	public override string NPCName => "Riot";
	public override int BaseHealth => 25;
	public override float BaseSpeed => 15;
	public override string BaseModel => "models/citizen/citizen.vmdl";
	public override int[] MinMaxCashReward => new int[] { 1, 10 };
	public override int[] MinMaxEXPReward => new int[] { 1, 5 };
	public override float NPCScale => 0.45f;
	public override SpecialType NPCType => SpecialType.Armoured;
	public override float ArmourStrength => 40.0f;
	public override float Damage => 10;

	public bool IsSplit;

	float armour;

	public override void Spawn()
	{
		base.Spawn();
		RenderColor = new Color( 170, 170, 170 );
		armour = ArmourStrength;

		var vest = new ModelEntity();
		vest.SetModel( "models/citizen_clothes/vest/tactical_vest/models/tactical_vest.vmdl" );
		vest.SetParent( this, true );
	}

	public override void TakeDamage( DamageInfo info )
	{
		armour -= info.Damage;

		if ( armour > 0 )
			return;

		base.TakeDamage( info );
	}
}

