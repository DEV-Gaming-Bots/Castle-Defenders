using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public partial class ZombieBoss : BaseNPC
{
	public override string NPCName => "Patient Zero";
	public override float BaseHealth => 135;
	public override float BaseSpeed => 9.75f;
	public override string BaseModel => "models/citizen/citizen.vmdl";
	public override int[] MinMaxCashReward => new int[] { 10, 65 };
	public override int[] MinMaxEXPReward => new int[] { 9, 35 };
	public override float NPCScale => 0.45f;
	public override float Damage => 5.0f;

	public override void Spawn()
	{
		base.Spawn();

		var coat = new ModelEntity( "models/citizen_clothes/jacket/labcoat.vmdl" );
		coat.SetParent( this, true );
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		SetMaterialOverride( Material.Load( "materials/npcs/zombie.vmat" ), "skin" );
	}
}

