using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public class SplitMinion : BaseNPC
{
	public string MinionName = "Minion";

	public override void SetUpPanel()
	{
		Panel = new NPCInfo( MinionName, Health );
	}

	public override void UpdateUI()
	{
		base.UpdateUI();
	}
}

