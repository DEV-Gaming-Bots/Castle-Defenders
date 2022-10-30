public sealed class SplitMinion : BaseNPC
{
	public string MinionName = "Minion";

	public override void SetUpPanel()
	{
		Panel = new NPCInfo( MinionName, Health );
	}
}

