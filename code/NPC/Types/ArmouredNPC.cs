using Sandbox;

public sealed partial class Riot : BaseNPC
{
	public override string NPCName => "Riot";
	public override float BaseHealth => 45;
	public override float BaseSpeed { get; set; } = 12.5f;
	public override string BaseModel => "models/citizen/citizen.vmdl";
	public override int[] MinMaxCashReward => new[] { 5, 20 };
	public override int[] MinMaxEXPReward => new[] { 1, 5 };
	public override float NPCScale => 0.45f;
	public override SpecialType NPCType => SpecialType.Armoured;
	public override float ArmourStrength => 30.0f;
	public override float Damage => 12.5f;

	[Net] public float Armour { get; set; }

	public override void Spawn()
	{
		base.Spawn();
		RenderColor = new Color( 170, 170, 170 );
		Armour = ArmourStrength;
		ArmourBroken = false;

		var vest = new ModelEntity( "models/citizen_clothes/vest/tactical_vest/models/tactical_vest.vmdl" );
		vest.SetParent( this, true );
		ClothingEnts.Add( vest );
	}

	public override void OnArmourBroken()
	{
		if ( ArmourBroken )
			return;

		//This doesn't work for some reason
		foreach ( var clothing in ClothingEnts.ToArray() )
		{
			clothing.SetParent( null, false );
			clothing.Delete();
		}

		BaseSpeed += 7.5f;

		base.OnArmourBroken();
	}

	public override void SetUpPanel()
	{
		Panel = new NPCInfo( NPCName, Health, "Armor: " + Armour );
	}

	public override void UpdateUI()
	{
		base.UpdateUI();

		if ( Armour <= 0 )
		{
			Panel.noteText = "Armor: 0";
			return;
		}

		Panel.noteText = "Armor: " + Armour;
	}

	public override void TakeDamage( DamageInfo info )
	{
		Armour -= info.Damage;

		if ( Armour > 0 )
			return;

		OnArmourBroken();

		base.TakeDamage( info );
	}
}

public sealed partial class Knight : BaseNPC
{
	public override string NPCName => "Knight";
	public override float BaseHealth => 95;
	public override float BaseSpeed { get; set; } = 10.0f;
	public override string BaseModel => "models/citizen/citizen.vmdl";
	public override int[] MinMaxCashReward => new[] { 13, 37 };
	public override int[] MinMaxEXPReward => new[] { 5, 20 };
	public override float NPCScale => 0.45f;
	public override SpecialType NPCType => SpecialType.Armoured;
	public override float ArmourStrength => 125.0f;
	public override float Damage => 15;

	[Net] public float Armour { get; set; }

	public override void Spawn()
	{
		base.Spawn();
		//RenderColor = new Color( 170, 170, 170 );
		Armour = ArmourStrength;
		ArmourBroken = false;

		var helmet = new ModelEntity( "models/citizen_clothes/hat/bucket_helmet/models/bucket_helmet.vmdl" );
		helmet.SetParent( this, true );

		var chainmail = new ModelEntity( "models/citizen_clothes/shirt/chainmail/models/chainmail.vmdl" );
		chainmail.SetParent( this, true );

		var chestplate = new ModelEntity( "models/citizen_clothes/vest/chest_armour/models/chest_armour.vmdl" );
		chestplate.SetParent( this, true );

		var trousers = new ModelEntity( "models/citizen_clothes/trousers/legarmour/models/leg_armour.vmdl" );
		trousers.SetParent( this, true );
	}

	public override void OnArmourBroken()
	{
		base.OnArmourBroken();

		if ( ArmourBroken )
			return;

		BaseSpeed += 2.5f;
	}
	public override void SetUpPanel()
	{
		Panel = new NPCInfo( NPCName, Health, "Armor: " + Armour );
	}

	public override void UpdateUI()
	{
		base.UpdateUI();

		if ( Armour <= 0 )
		{
			Panel.noteText = "Armor: 0";
			return;
		}

		Panel.noteText = "Armor: " + Armour;
	}
	public override void TakeDamage( DamageInfo info )
	{
		Armour -= info.Damage;

		if ( Armour > 0 )
			return;

		OnArmourBroken();

		base.TakeDamage( info );
	}
}

