using Sandbox.UI;
using Sandbox.UI.Construct;

public sealed class NPCInfo : WorldPanel
{
	public Label NpcInfo;
	public double CurHealth;
	public string NpcName;
	public string noteText;

	public NPCInfo(string name, double health, string noteText = "" )
	{
		StyleSheet.Load( "UI/NPCInfo.scss" );

		CurHealth = health;
		NpcName = name;
		this.noteText = noteText;

		NpcInfo = Add.Label( $"{NpcName}\nHealth: {CurHealth}\n{this.noteText}");
	}

	public override void Tick()
	{
		base.Tick();
		NpcInfo.SetText( $"{NpcName}\nHealth: {CurHealth}\n{noteText}" );
	}
}
