using Sandbox.UI;
using Sandbox.UI.Construct;

public  class NPCInfo : WorldPanel
{
	public Label NpcInfo;
	public double CurHealth;
	public string NpcName;
	public double CurArmor = -1;

	public NPCInfo(string name, double health, double armor = 0 )
	{
		StyleSheet.Load( "UI/NPCInfo.scss" );

		CurHealth = health;
		CurArmor = armor;
		NpcName = name;

		if( CurArmor != -1 )
			NpcInfo = Add.Label( $"{NpcName}\nHealth: {CurHealth}\n{CurArmor}" );
		else
			NpcInfo = Add.Label( $"{NpcName}\nHealth: {CurHealth}" );
	}

	public override void Tick()
	{
		base.Tick();

		if(CurArmor > -1)
			NpcInfo.SetText( $"{NpcName}\nHealth: {CurHealth}\n Armor: {CurArmor}" );
		else
			NpcInfo.SetText( $"{NpcName}\nHealth: {CurHealth}" );
	}
}
